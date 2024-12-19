using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApp.Models;
using System.Text.RegularExpressions;
using System.Net;

namespace WebApp.Controllers
{

    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                Response.Cookies.Delete("AuthToken");
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Validate the username to ensure it contains only letters and numbers
            if (username.Length > 10 || !Regex.IsMatch(username, "^[A-Za-z0-9]+$"))
            {
                ViewData["Error"] = "Username can only contain letters and numbers and be a maximum of 10 characters with no spaces.";
                return View();
            }

            if (password.Length < 15 || password.Length > 30 || !Regex.IsMatch(password, "^[A-Za-z0-9!#$]+$"))
            {
                ViewData["Error"] = "Password must be a minimum of 15 characters with a maximum of 30 and only contain letters, numbers, no spaces, and/or special characters including !,#, and $.";
                return View();
            }

            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            using (var httpClient = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://localhost:7232/api/User/login", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["Error"] = "Invalid credentials.";
                    return View();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string token = tokenResponse?.token;
                token = token?.Trim('"');

                if (string.IsNullOrEmpty(token))
                {
                    ViewData["Error"] = "Login failed.";
                    return View();
                }

                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
                var claims = jwtToken.Claims.Select(claim => new Claim(claim.Type, claim.Value)).ToList();

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = jwtToken.ValidTo
                });

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (username.Length > 10 || !Regex.IsMatch(username, "^[A-Za-z0-9]+$"))
            {
                ViewData["Error"] = "Username can only contain letters and numbers and be a maximum of 10 characters with no spaces.";
                return View();
            }

            if (password.Length < 15 || password.Length > 30 || !Regex.IsMatch(password, "^[A-Za-z0-9!#$]+$"))
            {
                ViewData["Error"] = "Password must be a minimum of 15 characters with a maximum of 30 and only contain letters, numbers, no spaces, and/or special characters including !,#, and $.";
                return View();
            }

            var userRegister = new RegisterUser
            {
                Username = username,
                Password = password,
            };

            using (var httpClient = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(userRegister), Encoding.UTF8, "application/json");
                try
                {
                    var response = await httpClient.PostAsync("https://localhost:7232/api/User/register", jsonContent);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        ViewData["Error"] = $"an error occured during registration: {responseContent}";
                        return View();
                    }
                    ViewData["Success"] = "Registration Successful!";
                    return View();
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = $"an exception occurred: {ex.Message}";
                    return View();
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminRegister()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> AdminRegister(string username, string password)
        {
            if (username.Length > 10 || !Regex.IsMatch(username, "^[A-Za-z0-9]+$"))
            {
                ViewData["Error"] = "Username can only contain letters and numbers and be a maximum of 10 characters with no spaces.";
                return View();
            }

            if (password.Length < 15 || password.Length > 30 || !Regex.IsMatch(password, "^[A-Za-z0-9!#$]+$"))
            {
                ViewData["Error"] = "Password must be a minimum of 15 characters with a maximum of 30 and only contain letters, numbers, no spaces, and/or special characters including !,#, and $.";
                return View();
            }

            var userRegister = new RegisterUser
            {
                Username = username,
                Password = password
            };

            using (var httpClient = new HttpClient())
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    ViewData["Error"] = "You are not authenticated.";
                    return View();
                }
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(userRegister), Encoding.UTF8, "application/json");
                try
                {
                    var response = await httpClient.PostAsync("https://localhost:7232/api/User/registerAdministrator", jsonContent);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        ViewData["Error"] = $"an error occured whilst registering the administrator: {responseContent}";
                        return View();
                    }
                    ViewData["Success"] = "Administrator registered successfully!";
                    return View();
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = $"Exception occurred: {ex.Message}";
                    return View();
                }
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UsersView()
        {
            List<User> users = new List<User>();
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = "You are not authenticated.";
                return View();
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7232/api/User");

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["Error"] = $"an error occurred whilst retrieving all the users, Status Code: {response.StatusCode}";
                    return View();
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    ViewData["Error"] = "the data returned from the response is either null or empty.";
                    return View();
                }

                users = JsonConvert.DeserializeObject<List<User>>(jsonResponse);
                if (users == null)
                {
                    ViewData["Error"] = "an error occured whislt deserializing the user data.";
                    return View();
                }
            }
            catch (HttpRequestException ex)
            {
                ViewData["Error"] = $"an error occured in the http request: {ex.Message}";
                return View();
            }


            return View(users);

        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUserView(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = "You are not authenticated.";
                return View();
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.DeleteAsync($"https://localhost:7232/api/User/{id}");

                if (response.IsSuccessStatusCode)
                {
                    ViewData["Success"] = "User deleted successfully!";
                }
                else
                {
                    ViewData["Error"] = $"Error deleting User with ID {id}. Status Code: {response.StatusCode}";
                }

                return View();
            }
            catch (HttpRequestException ex)
            {
                ViewData["Error"] = $"an error occured in the http request: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login", "Account");
        }
    }

}

