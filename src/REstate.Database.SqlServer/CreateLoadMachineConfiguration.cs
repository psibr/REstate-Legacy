using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Configuration")]
    [Tags("Full")]
    [Migration(5)]
    public class CreateLoadMachineConfiguration
        : Migration
    {
        public override void Up()
        {
            IfDatabase("SqlServer").Execute
                .EmbeddedScript("CreateLoadMachineConfiguration.sql");
        }

        public override void Down()
        {
            IfDatabase("SqlServer").Execute
                .Sql("DROP PROCEDURE [dbo].[LoadMachineDefinition];");
        }
    }
}