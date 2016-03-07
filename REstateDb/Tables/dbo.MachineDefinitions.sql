SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MachineDefinitions] (
		[MachineDefinitionId]                 [int] IDENTITY(1, 1) NOT NULL,
		[MachineName]                         [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[MachineDescription]                  [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[InitialStateName]                    [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[AutoIgnoreNotConfiguredTriggers]     [bit] NOT NULL,
		[IsActive]                            [bit] NOT NULL,
		CONSTRAINT [PK_Machines]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MachineDefinitions]
	ADD
	CONSTRAINT [CK_MachineDefinitions]
	CHECK
	([IsActive]=(0) OR [InitialStateName] IS NOT NULL)
GO
ALTER TABLE [dbo].[MachineDefinitions]
CHECK CONSTRAINT [CK_MachineDefinitions]
GO
ALTER TABLE [dbo].[MachineDefinitions]
	ADD
	CONSTRAINT [DF_Machines_AutoIgnoreNotConfiguredTriggers]
	DEFAULT ((0)) FOR [AutoIgnoreNotConfiguredTriggers]
GO
ALTER TABLE [dbo].[MachineDefinitions]
	ADD
	CONSTRAINT [DF_Machines_IsActive]
	DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[MachineDefinitions]
	WITH CHECK
	ADD CONSTRAINT [FK_MachineDefinitions_MachineDefinitions]
	FOREIGN KEY ([MachineDefinitionId]) REFERENCES [dbo].[MachineDefinitions] ([MachineDefinitionId])
ALTER TABLE [dbo].[MachineDefinitions]
	CHECK CONSTRAINT [FK_MachineDefinitions_MachineDefinitions]

GO
ALTER TABLE [dbo].[MachineDefinitions] SET (LOCK_ESCALATION = TABLE)
GO
