SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PrincipalClaims] (
		[PrincipalId]     [int] NOT NULL,
		[ClaimName]       [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[IsActive]        [bit] NOT NULL,
		CONSTRAINT [PK_PrincipalClaims]
		PRIMARY KEY
		CLUSTERED
		([PrincipalId], [ClaimName])
	ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PrincipalClaims]
	ADD
	CONSTRAINT [DF_PrincipalClaims_IsActive]
	DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PrincipalClaims]
	WITH CHECK
	ADD CONSTRAINT [FK_PrincipalClaims_Claims]
	FOREIGN KEY ([ClaimName]) REFERENCES [dbo].[Claims] ([ClaimName])
ALTER TABLE [dbo].[PrincipalClaims]
	CHECK CONSTRAINT [FK_PrincipalClaims_Claims]

GO
ALTER TABLE [dbo].[PrincipalClaims]
	WITH CHECK
	ADD CONSTRAINT [FK_PrincipalClaims_Principals]
	FOREIGN KEY ([PrincipalId]) REFERENCES [dbo].[Principals] ([PrincipalId])
ALTER TABLE [dbo].[PrincipalClaims]
	CHECK CONSTRAINT [FK_PrincipalClaims_Principals]

GO
ALTER TABLE [dbo].[PrincipalClaims] SET (LOCK_ESCALATION = TABLE)
GO
