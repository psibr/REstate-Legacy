SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[States] (
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ParentStateName]         [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[StateDescription]        [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		CONSTRAINT [PK_States_MachineDefinitionId_StateName]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId], [StateName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[States]
	WITH CHECK
	ADD CONSTRAINT [FK_States_MachineDefinitions]
	FOREIGN KEY ([MachineDefinitionId]) REFERENCES [dbo].[MachineDefinitions] ([MachineDefinitionId])
ALTER TABLE [dbo].[States]
	CHECK CONSTRAINT [FK_States_MachineDefinitions]

GO
EXEC sp_addextendedproperty N'MS_Description', N'A state record describes a state for a State Machine', 'SCHEMA', N'dbo', 'TABLE', N'States', 'CONSTRAINT', N'FK_States_MachineDefinitions'
GO
ALTER TABLE [dbo].[States]
	WITH CHECK
	ADD CONSTRAINT [FK_States_ParentState]
	FOREIGN KEY ([MachineDefinitionId], [ParentStateName]) REFERENCES [dbo].[States] ([MachineDefinitionId], [StateName])
ALTER TABLE [dbo].[States]
	CHECK CONSTRAINT [FK_States_ParentState]

GO
ALTER TABLE [dbo].[States] SET (LOCK_ESCALATION = TABLE)
GO
