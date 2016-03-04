SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IgnoreRules] (
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[IsActive]                [bit] NOT NULL,
		CONSTRAINT [PK_IgnoreStateTrigger]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId], [StateName], [TriggerName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[IgnoreRules]
	ADD
	CONSTRAINT [DF_IgnoreRules_IsActive]
	DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[IgnoreRules]
	WITH CHECK
	ADD CONSTRAINT [FK_IgnoreRules_Triggers]
	FOREIGN KEY ([MachineDefinitionId], [TriggerName]) REFERENCES [dbo].[Triggers] ([MachineDefinitionId], [TriggerName])
ALTER TABLE [dbo].[IgnoreRules]
	CHECK CONSTRAINT [FK_IgnoreRules_Triggers]

GO
ALTER TABLE [dbo].[IgnoreRules] SET (LOCK_ESCALATION = TABLE)
GO
