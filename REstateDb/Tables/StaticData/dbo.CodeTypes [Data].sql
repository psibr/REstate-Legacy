SET IDENTITY_INSERT [dbo].[CodeTypes] ON 

INSERT [dbo].[CodeTypes] ([CodeTypeId], [CodeTypeName], [CodeTypeDescription]) VALUES (1, N'SqlScalarBool', N'A SQL based boolean query using Susanoo for SQL communication.')
INSERT [dbo].[CodeTypes] ([CodeTypeId], [CodeTypeName], [CodeTypeDescription]) VALUES (2, N'SqlAction', N'A SQL based action that has no meaningful return. Uses Susanoo for SQL communication.')
INSERT [dbo].[CodeTypes] ([CodeTypeId], [CodeTypeName], [CodeTypeDescription]) VALUES (3, N'CSharpPredicate', N'A script based predicate.')
INSERT [dbo].[CodeTypes] ([CodeTypeId], [CodeTypeName], [CodeTypeDescription]) VALUES (4, N'CSharpAction', N'A script based action')
SET IDENTITY_INSERT [dbo].[CodeTypes] OFF
