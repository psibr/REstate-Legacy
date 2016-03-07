CREATE TYPE [dbo].[TriggerTable]
AS TABLE (
		[MachineDefinitionId]     [int] NOT NULL,
		[TriggerName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerDescription]      [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[IsActive]                [bit] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[MachineDefinitionId], [TriggerName]
)
WITH (IGNORE_DUP_KEY = OFF)
)
GO
