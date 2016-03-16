using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Chrono")]
    [Tags("Full")]
    [Migration(3)]
    public class CreateBaselineChronoTables
        : Migration
    {
        public override void Up()
        {
            Create.Table("ChronoTriggers")
                .WithColumn("ChronoTriggerId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("MachineInstanceId").AsGuid().NotNullable()
                .WithColumn("StateName").AsString(255).NotNullable()
                .WithColumn("TriggerName").AsString(255).NotNullable()
                .WithColumn("Payload").AsString(4000)
                .WithColumn("FireAfter").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("ChronoTriggers");
        }
    }
}