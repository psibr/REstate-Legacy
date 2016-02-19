SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SqlDatabaseProviders] (
		[ProviderName]            [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ProviderValue]           [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ProviderDescription]     [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		CONSTRAINT [PK_SqlDatabaseProviders_1]
		PRIMARY KEY
		CLUSTERED
		([ProviderName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SqlDatabaseProviders] SET (LOCK_ESCALATION = TABLE)
GO
