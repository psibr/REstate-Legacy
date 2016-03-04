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
CREATE STATISTICS [Triggers_MachineDefinitionId]
	ON [dbo].[Triggers] ([MachineDefinitionId])
GO
ALTER TABLE [dbo].[Triggers] SET (LOCK_ESCALATION = TABLE)
GO
