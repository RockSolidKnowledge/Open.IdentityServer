$ErrorActionPreference = "Stop";

New-Item -ItemType Directory -Force -Path ./nuget

dotnet tool restore

pushd ./src/Open.IdentityModel
Invoke-Expression "./build.ps1 $args"
popd

pushd ./src/Storage
Invoke-Expression "./build.ps1 $args"
popd

pushd ./src/Open.IdentityServer
Invoke-Expression "./build.ps1 $args"
popd

pushd ./src/EntityFramework.Storage
Invoke-Expression "./build.ps1 $args"
popd

pushd ./src/EntityFramework
Invoke-Expression "./build.ps1 $args"
popd

pushd ./src/AspNetIdentity
Invoke-Expression "./build.ps1 $args"
popd
