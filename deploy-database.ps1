Param(
    [ValidateSet("Release", "Debug")]
    [string]$configuration = $(throw "Configuration mode (Debug/Release) required."),
    [ValidateSet("Db2", "DotConnectOracle",
        "Firebird", "Hana", "Jet", "MySql",
        "Oracle", "OracleManaged", "Postgres",
        "SQLite", "SqlServer", "SqlServer2000",
        "SqlServer2005", "SqlServer2008",
        "SqlServer2012", "SqlServer2014",
        "SqlServerCe")]
    [string]$provider = "SqlServer2014",
    [ValidateSet("migrate", "rollback", "rollback:all")]
    [string]$task = "migrate",
    [string]$connectionString = "REstate",
    [string]$configPath = ".\deploy-database.config",
    [string]$tag = "Full"
)

$TARGET = "src\REstate.Database.SqlServer\bin\" + $configuration + "\REstate.Database.SqlServer.dll"

Invoke-Expression "src\packages\FluentMigrator.Tools.1.6.2\tools\AnyCPU\40\Migrate --connectionString `"$connectionString`" --provider `"$provider`" --target `"$TARGET`" --tag `"$tag`" --task `"$task`" --configPath=`"deploy-database.config`""
Write-Host

exit $LASTEXITCODE