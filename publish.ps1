Function DotnetPublish($runtime, $standalone)
{
    if($standalone)
    {
        dotnet publish AndroidBackupUnpackerConsole/abu.csproj --configuration Release --runtime $runtime --self-contained  --output "publish/$runtime-standalone" /p:PublishSingleFile=true /p:PublishTrimmed=true
    }
    else
    {
        dotnet publish AndroidBackupUnpackerConsole/abu.csproj --configuration Release --runtime $runtime --no-self-contained  --output "publish/$runtime" /p:PublishSingleFile=true
    }
}

DotnetPublish "win-x64" $True
DotnetPublish "win-x86" $True

DotnetPublish "win-x64" $False
DotnetPublish "win-x86" $False