using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Chrono")]
    [Tags("Full")]
    [Migration(3)]
    public class CreateBaselineChronoTables
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("ChronoTriggers")
                .WithColumn("ChronoTriggerId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("MachineInstanceId").AsGuid().NotNullable()
                .WithColumn("StateName").AsAnsiString(255).NotNullable()
                .WithColumn("TriggerName").AsAnsiString(255).NotNullable()
                .WithColumn("Payload").AsAnsiString(4000).Nullable()
                .WithColumn("FireAfter").AsDateTime().NotNullable();
        }
    }
}