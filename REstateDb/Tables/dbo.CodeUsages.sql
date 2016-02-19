SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CodeUsages] (
		[CodeUsageId]              [int] IDENTITY(1, 1) NOT NULL,
		[CodeUsageName]            [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[CodeUsageDescription]     [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		CONSTRAINT [PK_CodeUsages]
		PRIMARY KEY
		CLUSTERED
		([CodeUsageId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CodeUsages] SET (LOCK_ESCALATION = TABLE)
GO
