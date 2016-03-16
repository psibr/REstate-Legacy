using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Configuration")]
    [Tags("Full")]
    [Migration(6)]
    public class CreateTableTypesAndProcs
        : Migration
    {
        public override void Up()
        {
            IfDatabase("SqlServer").Execute
                .Sql(@"CREATE TYPE [dbo].[StateTable] AS TABLE(
                        [MachineDefinitionId] [int] NOT NULL,
                        [StateName] [varchar](255) NOT NULL,
                        [ParentStateName] [varchar](255) NULL,
                        [StateDescription] [varchar](500) NULL,
                        PRIMARY KEY CLUSTERED 
                    (
                        [MachineDefinitionId] ASC,
                        [StateName] ASC
                    )WITH (IGNORE_DUP_KEY = OFF)
                    )");

            IfDatabase("SqlServer").Execute
                .Sql(@"CREATE TYPE [dbo].[TriggerTable] AS TABLE(
                        [MachineDefinitionId] [int] NOT NULL,
                        [TriggerName] [varchar](255) NOT NULL,
                        [TriggerDescription] [varchar](500) NULL,
                        [IsActive] [bit] NOT NULL,
                        PRIMARY KEY CLUSTERED 
                    (
                        [MachineDefinitionId] ASC,
                        [TriggerName] ASC
                    )WITH (IGNORE_DUP_KEY = OFF)
                    )");

            IfDatabase("SqlServer").Execute
                .Sql(@"CREATE TYPE [dbo].[TransitionTable] AS TABLE(
                        [MachineDefinitionId] [int] NOT NULL,
                        [StateName] [varchar](255) NOT NULL,
                        [TriggerName] [varchar](255) NOT NULL,
                        [ResultantStateName] [varchar](255) NOT NULL,
                        [GuardName] [varchar](255) NULL,
                        [IsActive] [bit] NOT NULL,
                        PRIMARY KEY CLUSTERED 
                    (
                        [MachineDefinitionId] ASC,
                        [StateName] ASC,
                        [TriggerName] ASC
                    )WITH (IGNORE_DUP_KEY = OFF)
                    )");

            IfDatabase("SqlServer").Execute
                .Sql(@"CREATE PROCEDURE [dbo].[DefineStates]
                            @States StateTable READONLY
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            INSERT INTO States(MachineDefinitionId, StateName, ParentStateName, StateDescription)
                            SELECT MachineDefinitionId, StateName, ParentStateName, StateDescription FROM @States

                            SELECT States.* FROM States
                            INNER JOIN @States s on s.MachineDefinitionId = States.MachineDefinitionId AND s.StateName = States.StateName
                        END");

            IfDatabase("SqlServer").Execute
                .Sql(@"CREATE PROCEDURE [dbo].[DefineTriggers]
                            @Triggers TriggerTable READONLY
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            INSERT INTO [Triggers](MachineDefinitionId, TriggerName, TriggerDescription, IsActive)
                            SELECT MachineDefinitionId, TriggerName, TriggerDescription, IsActive FROM @Triggers

                            SELECT [Triggers].* FROM [Triggers]
                            INNER JOIN @Triggers t on t.MachineDefinitionId = [Triggers].MachineDefinitionId AND t.TriggerName = [Triggers].TriggerName
                        END");

            IfDatabase("SqlServer").Execute
                .Sql(@"CREATE PROCEDURE [dbo].[DefineTransitions]
                            @Transitions TransitionTable READONLY
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            INSERT INTO Transitions(MachineDefinitionId, StateName, TriggerName, ResultantStateName, GuardName, IsActive)
                            SELECT MachineDefinitionId, StateName, TriggerName, ResultantStateName, GuardName, IsActive FROM @Transitions

                            SELECT Transitions.* FROM Transitions
                            INNER JOIN @Transitions t on t.MachineDefinitionId = Transitions.MachineDefinitionId 
                                AND t.StateName = Transitions.StateName
                                AND t.TriggerName = Transitions.TriggerName
                        END");
        }

        public override void Down()
        {
            IfDatabase("SqlServer").Execute
                .Sql("DROP PROCEDURE [dbo].[DefineTransitions]");

            IfDatabase("SqlServer").Execute
                .Sql("DROP PROCEDURE [dbo].[DefineTriggers]");

            IfDatabase("SqlServer").Execute
                .Sql("DROP PROCEDURE [dbo].[DefineStates]");

            IfDatabase("SqlServer").Execute
                .Sql("DROP TYPE [dbo].[TransitionTable];");

            IfDatabase("SqlServer").Execute
                .Sql("DROP TYPE [dbo].[TriggerTable];");

            IfDatabase("SqlServer").Execute
                .Sql("DROP TYPE [dbo].[StateTable];");
        }
    }
}