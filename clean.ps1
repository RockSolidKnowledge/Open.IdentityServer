git clean -xdf -e samples

Remove-Item $env:USERPROFILE\.nuget\packages\identityserver4\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\identityserver4.storage\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\identityserver4.entityframework\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\identityserver4.entityframework.storage\ -Recurse -ErrorAction SilentlyContinue 
Remove-Item $env:USERPROFILE\.nuget\packages\identityserver4.aspnetidentity\ -Recurse -ErrorAction SilentlyContinue 