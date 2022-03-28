## API for managing personal expenses

Built with .NET6 using Entity Framework and PostgreSQL database
 ## Working with the Entity Framework
 ### Initial setup
 To set up the database structure, the created migrations should be run:
 1. Open Packet Manager Console
 2. Run `dotnet ef database update`

This will create all the necessary tables and constraints for the API.

### Useful Entity Framework commands
- Add a new migration: `dotnet ef migrations add <name>`
- Apply all code changes to database (run the existing migrations): `dotnet ef database update`
- Remove the last migration: `ef migrations remove`
 
 
 
