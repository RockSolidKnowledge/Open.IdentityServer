dotnet ef database drop -c PersistedGrantDbContext

rmdir /S /Q Migrations

dotnet ef migrations add Grants -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
dotnet ef migrations add Config -c ConfigurationDbContext -o Migrations/ConfigurationDb

dotnet ef migrations script -c PersistedGrantDbContext -o Migrations/PersistedGrantDb.sql
dotnet ef migrations script -c ConfigurationDbContext -o Migrations/ConfigurationDb.sql

dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext 
