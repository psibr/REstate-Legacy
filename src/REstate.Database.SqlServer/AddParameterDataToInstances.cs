using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Core")]
    [Tags("Full")]
    [Migration(2016060801)]
    public class AddParameterDataToInstances
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("Instances")
                .AddColumn("ParameterData").AsAnsiString(4000).Nullable();
        }
    }
}