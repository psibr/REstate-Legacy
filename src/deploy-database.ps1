Param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Debug",
    [string]$connectionString = "REstate",
    [string]$tag = "Full"
)

$TARGET = "src\REstate.Database.SqlServer\bin\" + $Configuration + "\REstate.Database.SqlServer.dll"


Invoke-Expression "src\packages\FluentMigrator.Tools.1.6.1\tools\AnyCPU\40\Migrate --connectionString `"$connectionString`" --provider SqlServer2014 --target `"$configuration`" --tag `"$tag`""
Write-Host

exit $LASTEXITCODE