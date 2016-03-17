param (
    [string]$connectionString = "server=(LocalDB)\MSSQLLocalDb;Application Name=REstateDeploy;Integrated Security=sspi;",
    [switch]$isNotSQLServer
)

function Get-DatabaseData {
    param (
        [string]$connectionString,
        [string]$query,
        [switch]$isNotSQLServer
    )
    if (!$isNotSQLServer) {
        Write-Verbose 'in SQL Server mode'
        $connection = New-Object -TypeName System.Data.SqlClient.SqlConnection
    } else {
        Write-Verbose 'in OleDB mode'
        $connection = New-Object -TypeName System.Data.OleDb.OleDbConnection
    }
    $connection.ConnectionString = $connectionString
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    if (!$isNotSQLServer) {
        $adapter = New-Object -TypeName System.Data.SqlClient.SqlDataAdapter $command
    } else {
        $adapter = New-Object -TypeName System.Data.OleDb.OleDbDataAdapter $command
    }
    $dataset = New-Object -TypeName System.Data.DataSet
    $adapter.Fill($dataset)
    $dataset.Tables[0]
}
function Invoke-DatabaseQuery {
    [CmdletBinding()]
    param (
        [string]$connectionString,
        [string]$query,
        [switch]$isSQLServer
    )
    if (!$isNotSQLServer) {
        Write-Verbose 'in SQL Server mode'
        $connection = New-Object -TypeName System.Data.SqlClient.SqlConnection
    } else {
        Write-Verbose 'in OleDB mode'
        $connection = New-Object -TypeName System.Data.OleDb.OleDbConnection
    }
    $connection.ConnectionString = $connectionString
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $connection.Open()
    $command.ExecuteNonQuery()
    $connection.close()
}

Get-DatabaseData $connectionString "IF NOT (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = 'REstate' OR name = 'REstate'))) CREATE DATABASE REstate" $isNotSQLServer