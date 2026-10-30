# Task Management API

A compact CQRS example with MediatR, JWT role authorization, EF Core/SQLite and xUnit. Domain, Application, Infrastructure and API are separate projects. Dependencies point from API and Infrastructure towards Application and Domain.

## Run
Requires .NET 8 SDK. Set secrets locally (do not commit them):
```bash
export Jwt__Key="replace-with-a-random-secret-at-least-32-bytes-long"
export Demo__Password="choose-a-local-demo-password"
dotnet run --project TaskApi.csproj
```
Open `/swagger` on the URL printed by the app. POST `/auth/token` with `{"username":"admin","password":"your-password"}`. Send the returned token as `Authorization: Bearer <token>`. GET `/tasks` requires a token; POST `/tasks` (`{"title":"Review"}`) and PUT `/tasks/{id}/complete` require Admin. Run tests with `dotnet test Tests/TaskApi.Tests.csproj`.

## Limitations
The single configured admin account is for local demonstrations only: no user management, password hashing, refresh tokens, rate limiting, HTTPS configuration, or per-user task ownership is included. Use a proper identity provider and secret store for production. SQLite is created automatically; use migrations for a real deployment. The API is a teaching sample.
