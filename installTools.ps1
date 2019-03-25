###########################################################################
# INSTALL SignTool
###########################################################################

$ToolPath = Join-Path $PSScriptRoot "tools"
$SignClientPath = Join-Path $ToolPath ".store\SignClient"
$SignClientExePath = (Get-ChildItem -Path $ToolPath -Filter "SignClient*" -File| ForEach-Object FullName | Select-Object -First 1)

if ((!(Test-Path -Path $SignClientPath -PathType Container)) -or (!(Test-Path $SignClientExePath -PathType Leaf))) {
    & dotnet tool install --tool-path $ToolPath SignClient
    if ($LASTEXITCODE -ne 0)
    {
        'Failed to install SignClient'
        exit 1
    }
    $SignClientExePath = (Get-ChildItem -Path $ToolPath -Filter "signtool*" -File| ForEach-Object FullName | Select-Object -First 1)
}