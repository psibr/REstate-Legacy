CREATE TYPE [dbo].[StateTable]
AS TABLE (
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ParentStateName]         [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[StateDescription]        [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	PRIMARY KEY CLUSTERED 
(
	[MachineDefinitionId], [StateName]
)
WITH (IGNORE_DUP_KEY = OFF)
)
GO
