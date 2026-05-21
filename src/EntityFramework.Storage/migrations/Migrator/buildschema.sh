DbProvider=SqlServer

[ -n "$1" ] && export DbProvider=$1
[ -n "$2" ] && export ConnectionStrings__db=$2
    
echo "Provider $DbProvider, Connection: $ConnectionStrings__db"
    
rm -rf Migrations
    
dotnet ef migrations add Grants -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
dotnet ef migrations add Configuration -c ConfigurationDbContext -o Migrations/ConfigurationDb
    
dotnet ef migrations script -c PersistedGrantDbContext -o scripts/$DbProvider/PersistedGrantDb.sql
dotnet ef migrations script -c ConfigurationDbContext -o scripts/$DbProvider/ConfigurationDb.sql

unset DbProvider
unset ConnectionStrings__db