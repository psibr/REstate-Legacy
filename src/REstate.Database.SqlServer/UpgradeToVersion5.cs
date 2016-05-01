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
    [Migration(20160430)]
    public class UpgradeToVersion5
        : Migration
    {
        public override void Up()
        {

            Create.Table("Machines")
                .WithColumn("MachineName").AsString(250).NotNullable().PrimaryKey()
                .WithColumn("ForkedFrom").AsString(250).Nullable().ForeignKey("Machines", "MachineName")
                .WithColumn("InitialState").AsString(250).NotNullable()
                .WithColumn("AutoIgnoreTriggers").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("Definition").AsString(4000).NotNullable()
                .WithColumn("CreatedDateTime").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("Instances")
                .WithColumn("InstanceId").AsString(250).NotNullable().PrimaryKey()
                .WithColumn("MachineName").AsString(250).NotNullable()
                .WithColumn("StateName").AsString(250).NotNullable().PrimaryKey()
                .WithColumn("CommitTag").AsString(250).NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("StateChangedDateTime").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);   
        }

        public override void Down()
        {
            Delete.Table("Machines");

            Delete.Table("Instances");
        }
    }
}
