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
                .WithColumn("Payload").AsString(4000).Nullable()
                .WithColumn("FireAfter").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("ChronoTriggers");
        }
    }

    [Tags("Chrono")]
    [Tags("Full")]
    [Migration(2016040700)]
    public class ChangePkOnChronoTables
    : Migration
    {
        public override void Up()
        {
            Delete.Table("ChronoTriggers");

            Create.Table("ChronoTriggers")
                .WithColumn("ChronoTriggerId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("MachineInstanceId").AsGuid().NotNullable()
                .WithColumn("StateName").AsString(255).NotNullable()
                .WithColumn("TriggerName").AsString(255).NotNullable()
                .WithColumn("Payload").AsString(4000).Nullable()
                .WithColumn("FireAfter").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("ChronoTriggers");

            Create.Table("ChronoTriggers")
                .WithColumn("ChronoTriggerId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("MachineInstanceId").AsGuid().NotNullable()
                .WithColumn("StateName").AsString(255).NotNullable()
                .WithColumn("TriggerName").AsString(255).NotNullable()
                .WithColumn("Payload").AsString(4000).Nullable()
                .WithColumn("FireAfter").AsDateTime().NotNullable();
        }
    }
}