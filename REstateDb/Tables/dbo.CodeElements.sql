SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CodeElements] (
		[CodeElementId]               [int] IDENTITY(1, 1) NOT NULL,
		[CodeTypeId]                  [int] NOT NULL,
		[CodeElementName]             [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[SemanticVersion]             [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[CodeElementDescription]      [varchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[CodeBody]                    [varchar](4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[SqlDatabaseDefinitionId]     [int] NULL,
		CONSTRAINT [PK_CodeElements]
		PRIMARY KEY
		CLUSTERED
		([CodeElementId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CodeElements]
	WITH CHECK
	ADD CONSTRAINT [FK_CodeElements_CodeTypes]
	FOREIGN KEY ([CodeTypeId]) REFERENCES [dbo].[CodeTypes] ([CodeTypeId])
ALTER TABLE [dbo].[CodeElements]
	CHECK CONSTRAINT [FK_CodeElements_CodeTypes]

GO
ALTER TABLE [dbo].[CodeElements]
	WITH CHECK
	ADD CONSTRAINT [FK_CodeElements_SqlDatabaseDefinitions]
	FOREIGN KEY ([SqlDatabaseDefinitionId]) REFERENCES [dbo].[SqlDatabaseDefinitions] ([SqlDatabaseDefinitionId])
ALTER TABLE [dbo].[CodeElements]
	CHECK CONSTRAINT [FK_CodeElements_SqlDatabaseDefinitions]

GO
ALTER TABLE [dbo].[CodeElements] SET (LOCK_ESCALATION = TABLE)
GO
