$ErrorActionPreference = "Stop";

New-Item -ItemType Directory -Force -Path ./nuget

dotnet tool restore

pushd ./src/Storage
./build.ps1 $args
popd

pushd ./src/IdentityServer4
./build.ps1 $args
popd

pushd ./src/EntityFramework.Storage
./build.ps1 $args
popd

pushd ./src/EntityFramework
./build.ps1 $args
popd

pushd ./src/AspNetIdentity
./build.ps1 $args
popd
