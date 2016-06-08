using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Core")]
    [Tags("Full")]
    [Migration(2016052000)]
    public class AddTriggerToInstances
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("Instances")
                .AddColumn("TriggerName").AsAnsiString(250).Nullable();
        }
    }
}