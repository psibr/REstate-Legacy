SET IDENTITY_INSERT [dbo].[CodeUsages] ON 

INSERT [dbo].[CodeUsages] ([CodeUsageId], [CodeUsageName], [CodeUsageDescription]) VALUES (1, N'Guard', N'Code that returns a boolean value to indicate if that state change is allowed.')
INSERT [dbo].[CodeUsages] ([CodeUsageId], [CodeUsageName], [CodeUsageDescription]) VALUES (2, N'Action', N'Code that performs an arbitrary action without a meaningful return value. eg. OnEntry, OnEntryFrom, and OnExit')
SET IDENTITY_INSERT [dbo].[CodeUsages] OFF
