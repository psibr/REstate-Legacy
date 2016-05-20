using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Core")]
    [Tags("Full")]
    [Migration(2016052000)]
    public class AddTriggerToInstances
        : Migration
    {
        public override void Up()
        {
            Alter.Table("Instances")
                .AddColumn("TriggerName").AsString(250).Nullable();
        }

        public override void Down()
        {
            Delete.Column("TriggerName")
                .FromTable("Instances");
        }
    }
}