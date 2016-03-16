using FluentMigrator;
using FluentMigrator.Builders.Alter.Column;

namespace REstate.Database.SqlServer
{
    [Tags("Instances")]
    [Tags("Full")]
    [Migration(2)]
    public class CreateBaselineInstancesTables
        : Migration
    {
        public override void Up()
        {
            Create.Table("MachineInstances")
                .WithColumn("MachineInstanceId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable()
                .WithColumn("StateName").AsString(255).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("MachineInstances");
        }
    }
}