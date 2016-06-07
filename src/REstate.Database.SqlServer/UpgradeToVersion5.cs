using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Core")]
    [Tags("Full")]
    [Migration(2016043001)]
    public class UpgradeToVersion5
        : ForwardOnlyMigration
    {
        public override void Up()
        {

            Create.Table("Machines")
                .WithColumn("MachineName").AsAnsiString(250).NotNullable().PrimaryKey()
                .WithColumn("ForkedFrom").AsAnsiString(250).Nullable().ForeignKey("Machines", "MachineName")
                .WithColumn("InitialState").AsAnsiString(250).NotNullable()
                .WithColumn("AutoIgnoreTriggers").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("Definition").AsAnsiString(4000).NotNullable()
                .WithColumn("CreatedDateTime").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("Instances")
                .WithColumn("InstanceId").AsAnsiString(250).NotNullable().PrimaryKey()
                .WithColumn("MachineName").AsAnsiString(250).NotNullable()
                .WithColumn("StateName").AsAnsiString(250).NotNullable().PrimaryKey()
                .WithColumn("CommitTag").AsAnsiString(250).NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("StateChangedDateTime").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);   
        }
    }
}
