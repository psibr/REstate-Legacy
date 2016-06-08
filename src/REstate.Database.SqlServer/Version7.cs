using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Core")]
    [Tags("Full")]
    [Migration(2016052401)]
    public class Version7
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("Instances")
                .AddColumn("Metadata").AsAnsiString(4000).Nullable();
        }
    }
}