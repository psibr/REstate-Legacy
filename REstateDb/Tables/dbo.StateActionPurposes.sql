SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StateActionPurposes] (
		[PurposeName]     [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Description]     [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		CONSTRAINT [PK_StateActionPurposes]
		PRIMARY KEY
		CLUSTERED
		([PurposeName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[StateActionPurposes] SET (LOCK_ESCALATION = TABLE)
GO
