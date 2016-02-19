SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StateActions] (
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[PurposeName]             [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[StateActionName]         [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[CodeElementId]           [int] NOT NULL,
		CONSTRAINT [PK_StateCodeTargets]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId], [StateName], [PurposeName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[StateActions]
	ADD
	CONSTRAINT [CK_StateActions_PurposeAndTriggerName]
	CHECK
	([PurposeName]<>'OnEntryFrom' AND [TriggerName] IS NULL OR [PurposeName]='OnEntryFrom' AND [TriggerName] IS NOT NULL)
GO
ALTER TABLE [dbo].[StateActions]
CHECK CONSTRAINT [CK_StateActions_PurposeAndTriggerName]
GO
ALTER TABLE [dbo].[StateActions]
	WITH CHECK
	ADD CONSTRAINT [FK_StateActions_CodeElements]
	FOREIGN KEY ([CodeElementId]) REFERENCES [dbo].[CodeElements] ([CodeElementId])
ALTER TABLE [dbo].[StateActions]
	CHECK CONSTRAINT [FK_StateActions_CodeElements]

GO
ALTER TABLE [dbo].[StateActions]
	WITH CHECK
	ADD CONSTRAINT [FK_StateActions_StateActionPurposes]
	FOREIGN KEY ([PurposeName]) REFERENCES [dbo].[StateActionPurposes] ([PurposeName])
ALTER TABLE [dbo].[StateActions]
	CHECK CONSTRAINT [FK_StateActions_StateActionPurposes]

GO
ALTER TABLE [dbo].[StateActions]
	WITH CHECK
	ADD CONSTRAINT [FK_StateCodeTargets_MachineDefinitions]
	FOREIGN KEY ([MachineDefinitionId]) REFERENCES [dbo].[MachineDefinitions] ([MachineDefinitionId])
ALTER TABLE [dbo].[StateActions]
	CHECK CONSTRAINT [FK_StateCodeTargets_MachineDefinitions]

GO
ALTER TABLE [dbo].[StateActions]
	WITH CHECK
	ADD CONSTRAINT [FK_StateCodeTargets_States]
	FOREIGN KEY ([MachineDefinitionId], [StateName]) REFERENCES [dbo].[States] ([MachineDefinitionId], [StateName])
ALTER TABLE [dbo].[StateActions]
	CHECK CONSTRAINT [FK_StateCodeTargets_States]

GO
ALTER TABLE [dbo].[StateActions]
	WITH CHECK
	ADD CONSTRAINT [FK_StateCodeTargets_Triggers]
	FOREIGN KEY ([MachineDefinitionId], [TriggerName]) REFERENCES [dbo].[Triggers] ([MachineDefinitionId], [TriggerName])
ALTER TABLE [dbo].[StateActions]
	CHECK CONSTRAINT [FK_StateCodeTargets_Triggers]

GO
ALTER TABLE [dbo].[StateActions] SET (LOCK_ESCALATION = TABLE)
GO
