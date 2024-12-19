# Project

To run this project without errors, 
-firstly you need to create the database, use the "create Database.sql" file in the code to help with that in (I Used SQL Server Management Studio 20 for this project using a SQLEXPRESS server).
-secondly, after the database is created, you will need to make sure your connection string is set up correctly (in appsettings.json in the WebAPI project), then run migrations etc via package manager console.
Here are a few commands to assist with the migrations etc (will be shown in order line by line below).

dotnet ef Migrations add Create --project WebAPI
dotnet ef database update --project WebAPI

Also most of the code is barely commented because most of it should be easy enough to read and understand, i have commented some of the more harder to understand code including the example SqlI regex, Xss regex, CommandInjection Regex and finally, the middleware in the program.cs that checks bodies in requests sent to the API against the regexs and the size of the requests etc before the requests are sent to the controllers etc.
