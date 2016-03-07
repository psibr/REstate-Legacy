SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MachineInstances] (
		[MachineInstanceId]       [uniqueidentifier] NOT NULL,
		[MachineDefinitionId]     [int] NOT NULL,
		[StateName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		CONSTRAINT [PK_MachineInstances]
		PRIMARY KEY
		CLUSTERED
		([MachineInstanceId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MachineInstances]
	WITH CHECK
	ADD CONSTRAINT [FK_MachineInstances_MachineDefinitions]
	FOREIGN KEY ([MachineDefinitionId]) REFERENCES [dbo].[MachineDefinitions] ([MachineDefinitionId])
ALTER TABLE [dbo].[MachineInstances]
	CHECK CONSTRAINT [FK_MachineInstances_MachineDefinitions]

GO
ALTER TABLE [dbo].[MachineInstances]
	WITH CHECK
	ADD CONSTRAINT [FK_MachineInstances_States]
	FOREIGN KEY ([MachineDefinitionId], [StateName]) REFERENCES [dbo].[States] ([MachineDefinitionId], [StateName])
	ON UPDATE CASCADE
ALTER TABLE [dbo].[MachineInstances]
	CHECK CONSTRAINT [FK_MachineInstances_States]

GO
ALTER TABLE [dbo].[MachineInstances] SET (LOCK_ESCALATION = TABLE)
GO
