SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[CodeTypeUsages] (
		[CodeTypeId]      [int] NOT NULL,
		[CodeUsageId]     [int] NOT NULL,
		CONSTRAINT [PK_CodeTypesToUsages]
		PRIMARY KEY
		CLUSTERED
		([CodeTypeId], [CodeUsageId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CodeTypeUsages]
	WITH CHECK
	ADD CONSTRAINT [FK_CodeTypeUsages_CodeTypes]
	FOREIGN KEY ([CodeTypeId]) REFERENCES [dbo].[CodeTypes] ([CodeTypeId])
ALTER TABLE [dbo].[CodeTypeUsages]
	CHECK CONSTRAINT [FK_CodeTypeUsages_CodeTypes]

GO
ALTER TABLE [dbo].[CodeTypeUsages]
	WITH CHECK
	ADD CONSTRAINT [FK_CodeTypeUsages_CodeUsages]
	FOREIGN KEY ([CodeUsageId]) REFERENCES [dbo].[CodeUsages] ([CodeUsageId])
ALTER TABLE [dbo].[CodeTypeUsages]
	CHECK CONSTRAINT [FK_CodeTypeUsages_CodeUsages]

GO
ALTER TABLE [dbo].[CodeTypeUsages] SET (LOCK_ESCALATION = TABLE)
GO
