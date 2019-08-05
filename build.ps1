New-Item -ItemType Directory -Force -Path ./nuget
dotnet tool install --tool-path tools SignClient

$cd = Get-Location

# storage
""
"###########################################"
"######### IdentityServer4.Storage #########"
"###########################################"
""
set-location ./src/Storage
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

# core
""
"###########################################"
"######### IdentityServer4 #################"
"###########################################"
""
set-location ./src/IdentityServer4
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

# EF storage
""
"############################################################"
"######### IdentityServer4.EntityFramework.Storage ##########"
"############################################################"
""
set-location ./src/EntityFramework.Storage
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

# EF
""
"###################################################"
"######### IdentityServer4.EntityFramework #########"
"###################################################"
""
set-location ./src/EntityFramework
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

# aspid
""
"###################################################"
"######### IdentityServer4.AspNetIdentity ##########"
"###################################################"
""
set-location ./src/AspNetIdentity
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}