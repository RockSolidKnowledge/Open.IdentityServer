New-Item -ItemType Directory -Force -Path ./nuget
Remove-Item -Path ./nuget/*.*

$cd = Get-Location

# storage
"######### Storage #########"
set-location ./src/Storage
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\Storage\artifacts\packages\*.nupkg -Destination .\nuget

# EF storage
"######### EntityFramework Storage #########"
set-location ./src/EntityFramework.Storage
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\EntityFramework.Storage\artifacts\packages\*.nupkg -Destination .\nuget

# EF
"######### EntityFramework #########"
set-location ./src/EntityFramework
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\EntityFramework\artifacts\packages\*.nupkg -Destination .\nuget

# core
"######### IdentityServer4 #########"
set-location ./src/IdentityServer4
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\IdentityServer4\artifacts\packages\*.nupkg -Destination .\nuget

# aspid
"######### ASP.NET Identity #########"
set-location ./src/AspNetIdentity
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\AspNetIdentity\artifacts\packages\*.nupkg -Destination .\nuget