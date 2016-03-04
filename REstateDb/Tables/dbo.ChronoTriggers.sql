SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ChronoTriggers] (
		[ChronoTriggerId]       [uniqueidentifier] NOT NULL,
		[MachineInstanceId]     [uniqueidentifier] NOT NULL,
		[StateName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[TriggerName]           [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Payload]               [varchar](2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[FireAfter]             [datetime2](7) NOT NULL,
		CONSTRAINT [PK_ChronoTriggers]
		PRIMARY KEY
		CLUSTERED
		([ChronoTriggerId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ChronoTriggers] SET (LOCK_ESCALATION = TABLE)
GO
