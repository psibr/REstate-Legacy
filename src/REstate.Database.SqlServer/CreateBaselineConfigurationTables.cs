using System;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Configuration")]
    [Tags("Full")]
    [Migration(1)]
    public class CreateBaselineConfigurationTables
        : Migration
    {
        public override void Up()
        {
            Create.Table("MachineDefinitions")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().Identity().PrimaryKey()
                .WithColumn("MachineName").AsString(255).NotNullable()
                .WithColumn("MachineDescription").AsString(500)
                .WithColumn("InitialStateName").AsString(255)
                .WithColumn("AutoIgnoreNotConfiguredTriggers").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Table("States")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("StateName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("ParentStateName").AsString(255)
                .WithColumn("StateDescription").AsString(500);

            Create.Table("Triggers")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("TriggerName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("TriggerDescription").AsString(500)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.Table("IgnoreRules")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("StateName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("TriggerName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.Table("SqlDatabaseDefinitions")
                .WithColumn("SqlDatabaseDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("SqlDatabaseName").AsString(255).NotNullable()
                .WithColumn("SqlDatabaseDescription").AsString(500)
                .WithColumn("ConnectionString").AsString(1000).NotNullable()
                .WithColumn("ProviderName").AsString(255).NotNullable();

            Create.Table("CodeElements")
                .WithColumn("CodeElementId").AsInt32().NotNullable().Identity().PrimaryKey()
                .WithColumn("ConnectorKey").AsString(255).NotNullable()
                .WithColumn("CodeElementName").AsString(255).NotNullable()
                .WithColumn("SemanticVersion").AsString(50).NotNullable().WithDefaultValue("0.1.0")
                .WithColumn("CodeElementDescription").AsString(1000)
                .WithColumn("CodeBody").AsString(4000).NotNullable()
                .WithColumn("SqlDatabaseDefinitionId").AsInt32();

            Create.Table("Guards")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("GuardName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("GuardDescription").AsString(500)
                .WithColumn("CodeElementId").AsInt32();

            Create.Table("Transitions")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("StateName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("TriggerName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("ResultantStateName").AsString(255).NotNullable()
                .WithColumn("GuardName").AsString(255)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.Table("StateActionPurposes")
                .WithColumn("PurposeName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("PurposeDescription").AsString(500);

            Create.Table("StateActions")
                .WithColumn("MachineDefinitionId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("StateName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("PurposeName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("TriggerName").AsString(255)
                .WithColumn("StateActionDescription").AsString(500)
                .WithColumn("CodeElementId").AsInt32().NotNullable();

            //States
            Create.ForeignKey("FK_States_ParentState_State")
                .FromTable("States").ForeignColumns("MachineDefinitionId", "ParentStateName")
                .ToTable("States").PrimaryColumns("MachineDefinitionId", "StateName");

            Create.ForeignKey("FK_States_MachineDefinitions")
                .FromTable("States").ForeignColumns("MachineDefinitionId")
                .ToTable("MachineDefinitions").PrimaryColumns("MachineDefinitionId");

            //Triggers
            Create.ForeignKey("FK_Triggers_MachineDefinitions")
                .FromTable("Triggers").ForeignColumns("MachineDefinitionId")
                .ToTable("MachineDefinitions").PrimaryColumns("MachineDefinitionId");

            //CodeElements
            Create.ForeignKey("FK_CodeElements_SqlDatabaseDefinitions")
                .FromTable("CodeElements").ForeignColumns("SqlDatabaseDefinitionId")
                .ToTable("SqlDatabaseDefinitions").PrimaryColumns("SqlDatabaseDefinitionId");

            //Guards
            Create.ForeignKey("FK_Guards_MachineDefinitions")
                .FromTable("Guards").ForeignColumns("MachineDefinitionId")
                .ToTable("MachineDefinitions").PrimaryColumns("MachineDefinitionId");

            Create.ForeignKey("FK_Guards_CodeElements")
                .FromTable("Guards").ForeignColumns("CodeElementId")
                .ToTable("CodeElements").PrimaryColumns("CodeElementId");

            //IgnoreRules
            Create.ForeignKey("FK_IgnoreRules_MachineDefinitions")
                .FromTable("IgnoreRules").ForeignColumns("MachineDefinitionId")
                .ToTable("MachineDefinitions").PrimaryColumns("MachineDefinitionId");

            Create.ForeignKey("FK_IgnoreRules_States")
                .FromTable("IgnoreRules").ForeignColumns("MachineDefinitionId", "StateName")
                .ToTable("States").PrimaryColumns("MachineDefinitionId", "StateName");

            Create.ForeignKey("FK_IgnoreRules_Triggers")
                .FromTable("IgnoreRules").ForeignColumns("MachineDefinitionId", "TriggerName")
                .ToTable("Triggers").PrimaryColumns("MachineDefinitionId", "TriggerName");

            //Transitions
            Create.ForeignKey("FK_Transitions_MachineDefinitions")
                .FromTable("Transitions").ForeignColumns("MachineDefinitionId")
                .ToTable("MachineDefinitions").PrimaryColumns("MachineDefinitionId");

            Create.ForeignKey("FK_Transitions_States")
                .FromTable("Transitions").ForeignColumns("MachineDefinitionId", "StateName")
                .ToTable("States").PrimaryColumns("MachineDefinitionId", "StateName");

            Create.ForeignKey("FK_Transitions_Triggers")
                .FromTable("Transitions").ForeignColumns("MachineDefinitionId", "TriggerName")
                .ToTable("Triggers").PrimaryColumns("MachineDefinitionId", "TriggerName");

            Create.ForeignKey("FK_Transitions_States_ResultantState")
                .FromTable("Transitions").ForeignColumns("MachineDefinitionId", "ResultantStateName")
                .ToTable("States").PrimaryColumns("MachineDefinitionId", "StateName");

            Create.ForeignKey("FK_Transitions_Guards")
                .FromTable("Transitions").ForeignColumns("MachineDefinitionId", "GuardName")
                .ToTable("Guards").PrimaryColumns("MachineDefinitionId", "GuardName");

            //StateActions
            Create.ForeignKey("FK_StateActions_States")
                .FromTable("StateActions").ForeignColumns("MachineDefinitionId", "StateName")
                .ToTable("States").PrimaryColumns("MachineDefinitionId", "StateName");

            Create.ForeignKey("FK_StateActions_Triggers")
                .FromTable("StateActions").ForeignColumns("MachineDefinitionId", "TriggerName")
                .ToTable("Triggers").PrimaryColumns("MachineDefinitionId", "TriggerName");

            Create.ForeignKey("FK_StateActions_CodeElements")
                .FromTable("StateActions").ForeignColumns("CodeElementId")
                .ToTable("CodeElements").PrimaryColumns("CodeElementId");

            IfDatabase("SqlServer").Execute
                .Sql(@"ALTER TABLE [dbo].[MachineDefinitions]  WITH CHECK ADD  CONSTRAINT [CK_MachineDefinitions] CHECK (([IsActive]=(0) OR [InitialStateName] IS NOT NULL));
                    ALTER TABLE[dbo].[MachineDefinitions] CHECK CONSTRAINT[CK_MachineDefinitions];");
        }

        public override void Down()
        {
            IfDatabase("SqlServer").Execute
                .Sql(@"ALTER TABLE [dbo].[MachineDefinitions] DROP CONSTRAINT [CK_MachineDefinitions]");

            //StateActions
            Delete.ForeignKey("FK_StateActions_States")
                .OnTable("StateActions");

            Delete.ForeignKey("FK_StateActions_Triggers")
                .OnTable("StateActions");

            Delete.ForeignKey("FK_StateActions_CodeElements")
                .OnTable("StateActions");

            //Transitions
            Delete.ForeignKey("FK_Transitions_MachineDefinitions")
                .OnTable("Transitions");

            Delete.ForeignKey("FK_Transitions_States_ResultantState")
                .OnTable("Transitions");

            Delete.ForeignKey("FK_Transitions_Triggers")
                .OnTable("Transitions");

            Delete.ForeignKey("FK_Transitions_States")
                .OnTable("Transitions");

            Delete.ForeignKey("FK_Transitions_Guards")
                .OnTable("Transitions");

            //IgnoreRules
            Delete.ForeignKey("FK_IgnoreRules_MachineDefinitions")
                .OnTable("IgnoreRules");

            Delete.ForeignKey("FK_IgnoreRules_States")
                .OnTable("IgnoreRules");

            Delete.ForeignKey("FK_IgnoreRules_Triggers")
                .OnTable("IgnoreRules");

            //Guards
            Delete.ForeignKey("FK_Guards_MachineDefinitions")
                .OnTable("Guards");

            Delete.ForeignKey("FK_Guards_CodeElements")
                .OnTable("Guards");

            //CodeElements
            Delete.ForeignKey("FK_CodeElements_SqlDatabaseDefinitions")
                .OnTable("CodeElements");

            //Triggers
            Delete.ForeignKey("FK_Triggers_MachineDefinitions")
                .OnTable("Triggers");

            //States
            Delete.ForeignKey("FK_States_ParentState_State")
                .OnTable("States");

            Delete.ForeignKey("FK_States_MachineDefinitions")
                .OnTable("States");
            
            Delete.Table("StateActions");
            Delete.Table("StateActionPurposes");
            Delete.Table("Transitions");
            Delete.Table("Guards");
            Delete.Table("CodeElements");
            Delete.Table("SqlDatabaseDefinitions");
            Delete.Table("IgnoreRules");
            Delete.Table("Triggers");
            Delete.Table("States");
            Delete.Table("MachineDefinitions");
        }
    }
}
