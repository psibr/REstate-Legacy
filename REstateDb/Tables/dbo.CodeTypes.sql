SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CodeTypes] (
		[CodeTypeId]              [int] IDENTITY(1, 1) NOT NULL,
		[CodeTypeName]            [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[CodeTypeDescription]     [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		CONSTRAINT [PK_GuardType]
		PRIMARY KEY
		CLUSTERED
		([CodeTypeId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CodeTypes] SET (LOCK_ESCALATION = TABLE)
GO
