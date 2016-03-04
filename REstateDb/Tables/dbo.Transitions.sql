SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Transitions] (
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ResultantStateName]      [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[GuardName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[IsActive]                [bit] NOT NULL,
		CONSTRAINT [PK_Transistion]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId], [StateName], [TriggerName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Transitions]
	ADD
	CONSTRAINT [DF_Transistions_IsActive]
	DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Transitions]
	WITH CHECK
	ADD CONSTRAINT [FK_Transitions_Guards]
	FOREIGN KEY ([MachineDefinitionId], [GuardName]) REFERENCES [dbo].[Guards] ([MachineDefinitionId], [GuardName])
ALTER TABLE [dbo].[Transitions]
	CHECK CONSTRAINT [FK_Transitions_Guards]

GO
ALTER TABLE [dbo].[Transitions]
	WITH CHECK
	ADD CONSTRAINT [FK_Transitions_ResultantStates]
	FOREIGN KEY ([MachineDefinitionId], [ResultantStateName]) REFERENCES [dbo].[States] ([MachineDefinitionId], [StateName])
	ON DELETE CASCADE
	ON UPDATE CASCADE
ALTER TABLE [dbo].[Transitions]
	CHECK CONSTRAINT [FK_Transitions_ResultantStates]

GO
ALTER TABLE [dbo].[Transitions]
	WITH CHECK
	ADD CONSTRAINT [FK_Transitions_States]
	FOREIGN KEY ([MachineDefinitionId], [StateName]) REFERENCES [dbo].[States] ([MachineDefinitionId], [StateName])
ALTER TABLE [dbo].[Transitions]
	CHECK CONSTRAINT [FK_Transitions_States]

GO
ALTER TABLE [dbo].[Transitions]
	WITH CHECK
	ADD CONSTRAINT [FK_Transitions_Triggers]
	FOREIGN KEY ([MachineDefinitionId], [TriggerName]) REFERENCES [dbo].[Triggers] ([MachineDefinitionId], [TriggerName])
	ON DELETE CASCADE
ALTER TABLE [dbo].[Transitions]
	CHECK CONSTRAINT [FK_Transitions_Triggers]

GO
ALTER TABLE [dbo].[Transitions] SET (LOCK_ESCALATION = TABLE)
GO
