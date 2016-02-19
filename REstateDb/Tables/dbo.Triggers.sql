SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Triggers] (
		[MachineDefinitionId]     [int] NOT NULL,
		[TriggerName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerDescription]      [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[IsActive]                [bit] NOT NULL,
		CONSTRAINT [PK_Triggers_MachineDefinitionId_TriggerName]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId], [TriggerName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Triggers]
	ADD
	CONSTRAINT [DF_Triggers_IsActive]
	DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Triggers]
	WITH CHECK
	ADD CONSTRAINT [FK_Triggers_MachineDefinitions]
	FOREIGN KEY ([MachineDefinitionId]) REFERENCES [dbo].[MachineDefinitions] ([MachineDefinitionId])
ALTER TABLE [dbo].[Triggers]
	CHECK CONSTRAINT [FK_Triggers_MachineDefinitions]

GO
EXEC sp_addextendedproperty N'MS_Description', N'A trigger record describes a possible trigger for a State Machine', 'SCHEMA', N'dbo', 'TABLE', N'Triggers', 'CONSTRAINT', N'FK_Triggers_MachineDefinitions'
GO
ALTER TABLE [dbo].[Triggers] SET (LOCK_ESCALATION = TABLE)
GO
