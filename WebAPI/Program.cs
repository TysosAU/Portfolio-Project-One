using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.RegularExpressions;
using WebAPI.Db;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//This regex matches a bunch of common sql keywords/fuctions used in sql injection regardless of being lower case or upper-case.
var SqlIRegex = new Regex(@"\b(select|insert|update|delete|drop|union|database|table|where|--|/\*|\*/|xp_|\bgo\b|\bexec\b|\bexecute\b|waitfor\s+delay|sleep|benchmark|concat)\b
        |(['"";#-]\s*--|/\*.*?\*/)|(\b(or|and)\b\s+[a-z0-9_=]+[^\w\s=]|--\s+[\r\n]+)
        |([']\s*=\s*['""""]?\s*(?:[0-9]{1,10}|[a-zA-Z0-9-_]+|\s*select\s+.*?))", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

//This regex matches a bunch of common keywords used in XSS injection.
var XssRegex = new Regex(@"<.*?(script|on\w+|style|iframe).*?>", RegexOptions.IgnoreCase);

//This regex matches a bunch of common keywords used in Command Injection.
var CommandInjectionRegex = new Regex(@"\b(&&|\|\||\||;|`|>|<|\\|exec|eval|sh|bash|cmd|powershell)\b", RegexOptions.IgnoreCase);

var connectionString = builder.Configuration.GetSection("DbConnection:ConnectionString").Value;
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HeroLoadoutService>();

var JwtIssuer = builder.Configuration["Jwt:Issuer"];
var JwtAudience = builder.Configuration["Jwt:Audience"];
var JwtSecretKey = builder.Configuration["Jwt:SecretKey"];

builder.Services.AddLogging();
builder.Services.AddSingleton(new JwtTokenService(JwtIssuer, JwtAudience, JwtSecretKey));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = JwtIssuer,
        ValidAudience = JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", builder =>
    {
        builder
        .WithOrigins("https://localhost:7072");
    });
});

builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ApiVersionReader = new QueryStringApiVersionReader("Api-Version");
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token.",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*
This will temporarily put the body in requests made to the API into memory 
to help conduct checks for if the body has a valid size and also if the body has valid inputs
that basically are not Sql Injection / Xss Injection / Command Injection 
attempts. If parts of the body match the regexes, return a 400 "Bad Request",
regardless of false positives etc, If the body to even begin with is above 3600 bytes, 
return a 413 "PayloadTooLarge". after the checks are completed the memory is disposed of and
sent to the garbage collector to help with performance. If the body in the request is valid,
the next middleware (for example, the relative controller) can process the request etc.
 */
app.Use(async (context, next) =>
{
    const int MaxBodySize = 3600; 

    if (context.Request.Body.CanRead)
    {
        context.Request.EnableBuffering(); 
        using (var memoryStream = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(memoryStream);
            context.Request.Body.Position = 0; 
            if (memoryStream.Length > MaxBodySize)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsync("Request body size is too big.");
                memoryStream.Dispose();
                GC.Collect(); 
                return;
            }
            var bodyContent = Encoding.UTF8.GetString(memoryStream.ToArray());
            if (SqlIRegex.IsMatch(bodyContent) || XssRegex.IsMatch(bodyContent) || CommandInjectionRegex.IsMatch(bodyContent))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Suspicious input.");
                bodyContent = null; 
                memoryStream.Dispose();
                GC.Collect();
                return;
            }
            bodyContent = null; 
        }
        GC.Collect(); 
    }
    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseHsts();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    DatabaseSeed.Seed(context);
}
app.Run();