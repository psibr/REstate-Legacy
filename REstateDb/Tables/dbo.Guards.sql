SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Guards] (
		[MachineDefinitionId]     [int] NOT NULL,
		[GuardName]               [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[GuardDescription]        [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[CodeElementId]           [int] NULL,
		CONSTRAINT [PK_Guards]
		PRIMARY KEY
		CLUSTERED
		([MachineDefinitionId], [GuardName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Guards]
	WITH CHECK
	ADD CONSTRAINT [FK_Guards_CodeElements]
	FOREIGN KEY ([CodeElementId]) REFERENCES [dbo].[CodeElements] ([CodeElementId])
ALTER TABLE [dbo].[Guards]
	CHECK CONSTRAINT [FK_Guards_CodeElements]

GO
ALTER TABLE [dbo].[Guards]
	WITH CHECK
	ADD CONSTRAINT [FK_Guards_MachineDefinitions]
	FOREIGN KEY ([MachineDefinitionId]) REFERENCES [dbo].[MachineDefinitions] ([MachineDefinitionId])
ALTER TABLE [dbo].[Guards]
	CHECK CONSTRAINT [FK_Guards_MachineDefinitions]

GO
ALTER TABLE [dbo].[Guards] SET (LOCK_ESCALATION = TABLE)
GO
