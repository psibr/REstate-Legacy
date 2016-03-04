CREATE TYPE [dbo].[TransitionTable]
AS TABLE (
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ResultantStateName]      [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[GuardName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[IsActive]                [bit] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[MachineDefinitionId], [StateName], [TriggerName]
)
WITH (IGNORE_DUP_KEY = OFF)
)
GO
