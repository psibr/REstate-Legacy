using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Chrono")]
    [Tags("Full")]
    [Migration(2016040700)]
    public class ChangePkOnChronoTables
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            Delete.Table("ChronoTriggers");

            Create.Table("ChronoTriggers")
                .WithColumn("ChronoTriggerId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("MachineInstanceId").AsGuid().NotNullable()
                .WithColumn("StateName").AsAnsiString(255).NotNullable()
                .WithColumn("TriggerName").AsAnsiString(255).NotNullable()
                .WithColumn("Payload").AsAnsiString(4000).Nullable()
                .WithColumn("FireAfter").AsDateTime().NotNullable();
        }
    }
}