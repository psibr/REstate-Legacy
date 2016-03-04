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
	ON DELETE CASCADE
ALTER TABLE [dbo].[States]
	CHECK CONSTRAINT [FK_States_MachineDefinitions]

GO
ALTER TABLE [dbo].[States]
	WITH CHECK
	ADD CONSTRAINT [FK_States_ParentState]
	FOREIGN KEY ([MachineDefinitionId], [ParentStateName]) REFERENCES [dbo].[States] ([MachineDefinitionId], [StateName])
ALTER TABLE [dbo].[States]
	CHECK CONSTRAINT [FK_States_ParentState]

GO
CREATE STATISTICS [States_MachineDefinitionId]
	ON [dbo].[States] ([MachineDefinitionId])
GO
CREATE STATISTICS [States_StateName]
	ON [dbo].[States] ([StateName])
GO
ALTER TABLE [dbo].[States] SET (LOCK_ESCALATION = TABLE)
GO
