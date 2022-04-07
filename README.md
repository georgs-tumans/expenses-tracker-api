## API for managing personal expenses

Built with .NET6 using Entity Framework and PostgreSQL database

## Project description
This is an API for managing personal expenses. Expenses are grouped under categories and are tied to users. Endpoints for managing each of these are provided. In addition to default categories that are the same for everyone, users can also create their own.

## Features
#### User types:
Two user types have been implemented. New user registration and login endpoints are provided.
- **Administrators:** the main difference is that administrator users can mostly access all data - expenses, categories, all user list. They can also perform CRUD operations on these data. Only administrators can elevate other users to administrator type (and remove the privilage). They can also delete other users and create new default categories.
- **Regular users:** users can only perform CRUD on their own expenses and categories.
#### Authorization
All endpoints are only accessible when authentificated as a registered user. To authentificate, the Login endpoint should be called which then returns a bearer token that must then be passed with every request. Token lifespan is two hours.

#### New user account activation
All new accounts registered must first be activated before logging in. This API includes account activation via a link sent to user email after account registration. The link calls an endpoint of the API which then redirects to an arbitrary link (two different redirects can be used for successful or failed account activation).

#### Logging
All operations and exceptions are logged to the database table `Weblogs`.

#### Versioning
A versioning system is implemented in code so additional API versions can be easily created by adding the attribute `[ApiVersion("x.x")]`.

### Swagger
This API is fully integrated with Swagger thus information on available endpoints, parameters and schemas is easily accessible. Authorization and API version selection can also be done via the Swagger GUI.

## Useful
### Running the project
1. Set up a local PostgreSQL server (or use an existing one)
2. Add an appsettings.json (see below)
3. Run database migrations (see below)

### Appsettings.json structure
In order to run the project, an appsettings.json should have the following structure:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "local": ""       //Connection string for a local PostgreSQL server
  },
  "AuthTokenKey": "", //Any generated token string that is used for generating auth tokens
  "EmailConfirmSuccessUrl": "", //Url for redirection after a successful account activation
  "EmailConfirmFailUrl": "", //Url for redirection after a failed account activation
  "TokenExpirationHours": , //Sets the expiration time of the account activation token (in hours)
  "SendingEmail": {
    "SmtpServer": "",
    "SmtpPort": ,
    "SenderAddress": "",  //The full email address of the user used for sending email
    "SenderLoginPassword": "" //Password of the sender account
  }
}
```
 ### Working with the Entity Framework
 #### Initial setup
 To set up the database structure, the created migrations should be run:
 1. Open Packet Manager Console
 2. Run `dotnet ef database update`

This will create all the necessary tables and constraints for the API.

#### Useful Entity Framework commands
- Add a new migration: `dotnet ef migrations add <name>`
- Apply all code changes to database (run the existing migrations): `dotnet ef database update`
- Remove the last migration: `ef migrations remove`
 
 
 
