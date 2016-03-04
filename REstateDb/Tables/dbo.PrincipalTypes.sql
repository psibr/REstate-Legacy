SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PrincipalTypes] (
		[PrincipalTypeName]            [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[PrincipalTypeDescription]     [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		CONSTRAINT [PK_PrincipalTypes]
		PRIMARY KEY
		CLUSTERED
		([PrincipalTypeName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PrincipalTypes] SET (LOCK_ESCALATION = TABLE)
GO
