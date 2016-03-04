SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SqlDatabaseDefinitions] (
		[SqlDatabaseDefinitionId]     [int] IDENTITY(1, 1) NOT NULL,
		[SqlDatabaseName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[SqlDatabaseDescription]      [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[ConnectionString]            [varchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[ProviderName]                [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		CONSTRAINT [PK_SqlDatabaseDefinitions]
		PRIMARY KEY
		CLUSTERED
		([SqlDatabaseDefinitionId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SqlDatabaseDefinitions]
	WITH CHECK
	ADD CONSTRAINT [FK_SqlDatabaseDefinitions_SqlDatabaseProviders]
	FOREIGN KEY ([ProviderName]) REFERENCES [dbo].[SqlDatabaseProviders] ([ProviderName])
ALTER TABLE [dbo].[SqlDatabaseDefinitions]
	CHECK CONSTRAINT [FK_SqlDatabaseDefinitions_SqlDatabaseProviders]

GO
ALTER TABLE [dbo].[SqlDatabaseDefinitions] SET (LOCK_ESCALATION = TABLE)
GO
