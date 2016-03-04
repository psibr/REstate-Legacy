SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Principals] (
		[PrincipalId]               [int] IDENTITY(1, 1) NOT NULL,
		[ApiKey]                    [uniqueidentifier] NOT NULL,
		[PrincipalType]             [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[UserOrApplicationName]     [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[PasswordHash]              [varchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
		[IsActive]                  [bit] NOT NULL,
		CONSTRAINT [PK_Principals]
		PRIMARY KEY
		CLUSTERED
		([PrincipalId])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Principals]
	ADD
	CONSTRAINT [DF_Principals_ApiKey]
	DEFAULT (newid()) FOR [ApiKey]
GO
ALTER TABLE [dbo].[Principals]
	ADD
	CONSTRAINT [DF_Principals_IsActive]
	DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Principals]
	WITH CHECK
	ADD CONSTRAINT [FK_Principals_PrincipalTypes]
	FOREIGN KEY ([PrincipalType]) REFERENCES [dbo].[PrincipalTypes] ([PrincipalTypeName])
ALTER TABLE [dbo].[Principals]
	CHECK CONSTRAINT [FK_Principals_PrincipalTypes]

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ApiKey]
	ON [dbo].[Principals] ([ApiKey])
	ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserOrApplicationName]
	ON [dbo].[Principals] ([UserOrApplicationName])
	ON [PRIMARY]
GO
ALTER TABLE [dbo].[Principals] SET (LOCK_ESCALATION = TABLE)
GO
