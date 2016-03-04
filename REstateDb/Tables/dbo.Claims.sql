SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Claims] (
		[ClaimName]            [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ClaimDescription]     [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		CONSTRAINT [PK_Claims]
		PRIMARY KEY
		CLUSTERED
		([ClaimName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Claims] SET (LOCK_ESCALATION = TABLE)
GO
