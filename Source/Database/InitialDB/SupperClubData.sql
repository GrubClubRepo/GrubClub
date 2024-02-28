USE [SupperClub]
GO
SET IDENTITY_INSERT [dbo].[UsageEventType] ON 

INSERT [dbo].[UsageEventType] ([ID], [Description], [Note]) VALUES (1, N'Login', N'Fired on successful user login')
INSERT [dbo].[UsageEventType] ([ID], [Description], [Note]) VALUES (2, N'Search', N'Fired when user runs a search')
INSERT [dbo].[UsageEventType] ([ID], [Description], [Note]) VALUES (3, N'Password Change', N'Fired when user either resets or changes password')
SET IDENTITY_INSERT [dbo].[UsageEventType] OFF
INSERT [dbo].[aspnet_Applications] ([ApplicationName], [LoweredApplicationName], [ApplicationId], [Description]) VALUES (N'/', N'/', N'8dbfddf4-ff68-4070-a25e-0fb45d802033', NULL)
INSERT [dbo].[aspnet_Users] ([ApplicationId], [UserId], [UserName], [LoweredUserName], [MobileAlias], [IsAnonymous], [LastActivityDate]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'admin', N'admin', NULL, 0, CAST(0x0000A0B400EF5724 AS DateTime))
INSERT [dbo].[aspnet_Users] ([ApplicationId], [UserId], [UserName], [LoweredUserName], [MobileAlias], [IsAnonymous], [LastActivityDate]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'1cb14c0f-3546-4e79-80d5-db848f8cc125', N'guest', N'guest', NULL, 0, CAST(0x0000A0B400EF85BB AS DateTime))
INSERT [dbo].[aspnet_Users] ([ApplicationId], [UserId], [UserName], [LoweredUserName], [MobileAlias], [IsAnonymous], [LastActivityDate]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'e443b1f3-3515-4d05-a5d3-029f5c96d9d8', N'host', N'host', NULL, 0, CAST(0x0000A0B400EF9FD7 AS DateTime))

INSERT [dbo].[User] ([Id], [FirstName], [LastName], [DateOfBirth], [Address], [Country], [PostCode], [Gender], [FBUserOnly]) VALUES (N'e443b1f3-3515-4d05-a5d3-029f5c96d9d8', N'Host', N'Host', CAST(0x00006F4900000000 AS DateTime), N'Test', N'UK', N'SW4', N'Male      ', 'False')
INSERT [dbo].[User] ([Id], [FirstName], [LastName], [DateOfBirth], [Address], [Country], [PostCode], [Gender], [FBUserOnly]) VALUES (N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'Admin', N'Admin', CAST(0x00006F8600000000 AS DateTime), N'Test', N'UK', N'SW4', N'Male      ', 'False')
INSERT [dbo].[User] ([Id], [FirstName], [LastName], [DateOfBirth], [Address], [Country], [PostCode], [Gender], [FBUserOnly]) VALUES (N'1cb14c0f-3546-4e79-80d5-db848f8cc125', N'Guest', N'Guest', CAST(0x00006F6900000000 AS DateTime), N'Test', N'UK', N'SW4', N'Male      ', 'False')
SET IDENTITY_INSERT [dbo].[SupperClub] ON 

INSERT [dbo].[SupperClub] ([Id], [UserId], [Name], [Summary], [Description], [Blog], [Twitter], [Facebook], [Pinterest], [TradingName], [Address], [Address2], [City], [Country], [PostCode], [Latitude], [Longitude], [ContactNumber], [VATNumber], [ChequeName], [BankName], [BankAddress], [SortCode], [AccountNumber], [HouseRule], [ImagePath], [CouncilRegistered], [FoodHygieneCertificate], [IndemnityInsurace], [AlcoholLicense], [Active])
VALUES (3, N'e443b1f3-3515-4d05-a5d3-029f5c96d9d8', N'Sample Supper Club 1', N'Come along to our regular Windsor venue, meeting for drinks in the Two Brewers', N'The lovetoshare.it is open all day, from 12 noon -11.30pm (until 10.30pm on Sunday)', N'http://www.blogspot.com', NULL, NULL, NULL, N'test11', N'123', N'123', N'London', N'UK', N'SW4 0EN', 51.4685833, -0.1416816, N'234234', N'24234', N'24234', N'234 ABC', N'afsadf sad', N'sdf', N'234234234', NULL, 'defaultSupperClubImage.png', 1, 1, 1, 1, 0)
INSERT [dbo].[SupperClub] ([Id], [UserId], [Name], [Summary], [Description], [Blog], [Twitter], [Facebook], [Pinterest], [TradingName], [Address], [Address2], [City], [Country], [PostCode], [Latitude], [Longitude], [ContactNumber], [VATNumber], [ChequeName], [BankName], [BankAddress], [SortCode], [AccountNumber], [HouseRule], [ImagePath], [CouncilRegistered], [FoodHygieneCertificate], [IndemnityInsurace], [AlcoholLicense], [Active])
VALUES (4, N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'Sample Supper Club 2', N'This is a short summary of the Supper Club', N'The lovetoshare.it is open all day, from 12 noon -11.30pm (until 10.30pm on Sunday)', N'http://www.blogspot.com', NULL, NULL, NULL, N'test11', N'123', N'123', N'London', N'UK', N'SW4 0EN', 51.4685833, -0.1416816, N'234234', N'24234', N'24234', N'234 ABC', N'afsadf sad', N'sdf', N'234234234', NULL, 'defaultSupperClubImage.png', 1, 1, 1, 1, 0)
SET IDENTITY_INSERT [dbo].[SupperClub] OFF
SET IDENTITY_INSERT [dbo].[Event] ON 

INSERT [dbo].[Event] ([Id], [Name], [Description], [Start], [End], [SupperClubId], [Guests], [Cost], [Alcohol], [Charity], [ImagePath], [DateCreated], [Active], [Address], [Address2], [City], [PostCode], [Latitude], [Longitude]) 
VALUES (2, N'Set Sunday Lunch & All Day Dining 1', N'xvxcvxc', CAST(0x0000A10700000000 AS DateTime), CAST(0x0000A10900000000 AS DateTime), 3, 10, 117.5000, 1, 0, N'defaultEventImage.png', CAST(0x0000A0B001196405 AS DateTime), 1, N'Address 1', N'Address 2', N'London', N'SW4 0EN', 51.4685833, -0.1416816)
INSERT [dbo].[Event] ([Id], [Name], [Description], [Start], [End], [SupperClubId], [Guests], [Cost], [Alcohol], [Charity], [ImagePath], [DateCreated], [Active], [Address], [Address2], [City], [PostCode], [Latitude], [Longitude]) 
VALUES (3, N'Set Sunday Lunch & All Day Dining 2', N'xcvxcv sgdfg', CAST(0x0000A10A00000000 AS DateTime), CAST(0x0000A10A00C5C100 AS DateTime), 3, 6, 25.5000, 0, 1, N'defaultEventImage.png', CAST(0x0000A0B001196405 AS DateTime), 1, N'Address 1', N'Address 2', N'London', N'SW4 0EN', 51.4685833, -0.1416816)
SET IDENTITY_INSERT [dbo].[Event] OFF

SET IDENTITY_INSERT [dbo].[Diet] ON 
INSERT [dbo].[Diet] ([Id], [Name]) VALUES (1, N'Vegetarian')
INSERT [dbo].[Diet] ([Id], [Name]) VALUES (2, N'Vegan')
SET IDENTITY_INSERT [dbo].[Diet] OFF 

SET IDENTITY_INSERT [dbo].[Cuisine] ON 

INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (1, N'English')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (3, N'Japanese')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (4, N'Italian')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (5, N'Thai')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (10, N'Chinese')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (14, N'Malaysian')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (27, N'Indonesian')
INSERT [dbo].[Cuisine] ([Id], [Name]) VALUES (33, N'Hungarian')
SET IDENTITY_INSERT [dbo].[Cuisine] OFF
SET IDENTITY_INSERT [dbo].[EventCuisine] ON 

INSERT [dbo].[EventCuisine] ([Id], [EventId], [CuisineId]) VALUES (1, 2, 1)
INSERT [dbo].[EventCuisine] ([Id], [EventId], [CuisineId]) VALUES (2, 2, 3)
SET IDENTITY_INSERT [dbo].[EventCuisine] OFF
SET IDENTITY_INSERT [dbo].[Menu] ON 

INSERT [dbo].[Menu] ([Id], [EventId], [MenuItem]) VALUES (1, 2, N'Prawn Cocktail')
INSERT [dbo].[Menu] ([Id], [EventId], [MenuItem]) VALUES (2, 3, N'Main Meal')
SET IDENTITY_INSERT [dbo].[Menu] OFF
INSERT [dbo].[aspnet_Membership] ([ApplicationId], [UserId], [Password], [PasswordFormat], [PasswordSalt], [MobilePIN], [Email], [LoweredEmail], [PasswordQuestion], [PasswordAnswer], [IsApproved], [IsLockedOut], [CreateDate], [LastLoginDate], [LastPasswordChangedDate], [LastLockoutDate], [FailedPasswordAttemptCount], [FailedPasswordAttemptWindowStart], [FailedPasswordAnswerAttemptCount], [FailedPasswordAnswerAttemptWindowStart], [Comment]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'lYve4UiQGpEB61VSEfuetc8B0lo=', 1, N'8z7j0mVP1WjyFuicNI2GIQ==', NULL, N'admin@sc.com', N'admin@sc.com', NULL, NULL, 1, 0, CAST(0x0000A0B400EF5650 AS DateTime), CAST(0x0000A0B400EF5724 AS DateTime), CAST(0x0000A0B400EF5650 AS DateTime), CAST(0xFFFF2FB300000000 AS DateTime), 0, CAST(0xFFFF2FB300000000 AS DateTime), 0, CAST(0xFFFF2FB300000000 AS DateTime), NULL)
INSERT [dbo].[aspnet_Membership] ([ApplicationId], [UserId], [Password], [PasswordFormat], [PasswordSalt], [MobilePIN], [Email], [LoweredEmail], [PasswordQuestion], [PasswordAnswer], [IsApproved], [IsLockedOut], [CreateDate], [LastLoginDate], [LastPasswordChangedDate], [LastLockoutDate], [FailedPasswordAttemptCount], [FailedPasswordAttemptWindowStart], [FailedPasswordAnswerAttemptCount], [FailedPasswordAnswerAttemptWindowStart], [Comment]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'1cb14c0f-3546-4e79-80d5-db848f8cc125', N'Pf3zwMuDpha5ClyyUgsWUgqg9b8=', 1, N'QbsHetT7uLqNwqnnR8Csow==', NULL, N'guest1@sc.com', N'guest1@sc.com', NULL, NULL, 1, 0, CAST(0x0000A0B400EF8530 AS DateTime), CAST(0x0000A0B400EF85BB AS DateTime), CAST(0x0000A0B400EF8530 AS DateTime), CAST(0xFFFF2FB300000000 AS DateTime), 0, CAST(0xFFFF2FB300000000 AS DateTime), 0, CAST(0xFFFF2FB300000000 AS DateTime), NULL)
INSERT [dbo].[aspnet_Membership] ([ApplicationId], [UserId], [Password], [PasswordFormat], [PasswordSalt], [MobilePIN], [Email], [LoweredEmail], [PasswordQuestion], [PasswordAnswer], [IsApproved], [IsLockedOut], [CreateDate], [LastLoginDate], [LastPasswordChangedDate], [LastLockoutDate], [FailedPasswordAttemptCount], [FailedPasswordAttemptWindowStart], [FailedPasswordAnswerAttemptCount], [FailedPasswordAnswerAttemptWindowStart], [Comment]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'e443b1f3-3515-4d05-a5d3-029f5c96d9d8', N'rFfwDxi2kMCbfZKXVSTRS5dieRQ=', 1, N'Gg4RArsH11G8OxDHDWIyFA==', NULL, N'host@sc.com', N'host@sc.com', NULL, NULL, 1, 0, CAST(0x0000A0B400EF9EF8 AS DateTime), CAST(0x0000A0B400EF9FD7 AS DateTime), CAST(0x0000A0B400EF9EF8 AS DateTime), CAST(0xFFFF2FB300000000 AS DateTime), 0, CAST(0xFFFF2FB300000000 AS DateTime), 0, CAST(0xFFFF2FB300000000 AS DateTime), NULL)
INSERT [dbo].[aspnet_Roles] ([ApplicationId], [RoleId], [RoleName], [LoweredRoleName], [Description]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'1e4215f2-afb5-43d5-96b4-895d38ef2f98', N'Admin', N'admin', NULL)
INSERT [dbo].[aspnet_Roles] ([ApplicationId], [RoleId], [RoleName], [LoweredRoleName], [Description]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'2e339e2f-9bd5-4420-89d9-91470a016fdd', N'Guest', N'guest', NULL)
INSERT [dbo].[aspnet_Roles] ([ApplicationId], [RoleId], [RoleName], [LoweredRoleName], [Description]) VALUES (N'8dbfddf4-ff68-4070-a25e-0fb45d802033', N'0418bc43-55ce-4d5f-81ee-df51616c4ee3', N'Host', N'host', NULL)
--Admin
INSERT [dbo].[aspnet_UsersInRoles] ([UserId], [RoleId]) VALUES (N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'2e339e2f-9bd5-4420-89d9-91470a016fdd')
INSERT [dbo].[aspnet_UsersInRoles] ([UserId], [RoleId]) VALUES (N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'0418bc43-55ce-4d5f-81ee-df51616c4ee3')
INSERT [dbo].[aspnet_UsersInRoles] ([UserId], [RoleId]) VALUES (N'3fc9ac18-08e4-4167-82f3-8c7fd959301b', N'1e4215f2-afb5-43d5-96b4-895d38ef2f98')
--Host
INSERT [dbo].[aspnet_UsersInRoles] ([UserId], [RoleId]) VALUES (N'e443b1f3-3515-4d05-a5d3-029f5c96d9d8', N'2e339e2f-9bd5-4420-89d9-91470a016fdd')
INSERT [dbo].[aspnet_UsersInRoles] ([UserId], [RoleId]) VALUES (N'e443b1f3-3515-4d05-a5d3-029f5c96d9d8', N'0418bc43-55ce-4d5f-81ee-df51616c4ee3')
--Guest
INSERT [dbo].[aspnet_UsersInRoles] ([UserId], [RoleId]) VALUES (N'1cb14c0f-3546-4e79-80d5-db848f8cc125', N'2e339e2f-9bd5-4420-89d9-91470a016fdd')

INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'common', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'health monitoring', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'membership', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'personalization', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'profile', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'role manager', N'1', 1)
SET IDENTITY_INSERT [dbo].[PriceRange] ON 

INSERT [dbo].[PriceRange] ([Id], [PriceName], [MinPrice], [MaxPrice], [Country]) VALUES (1, N'££ (£25 and under)', 0.0000, 25.0000, N'UK')
INSERT [dbo].[PriceRange] ([Id], [PriceName], [MinPrice], [MaxPrice], [Country]) VALUES (2, N'£££ (£26 to £40)', 26.0000, 40.0000, N'UK')
INSERT [dbo].[PriceRange] ([Id], [PriceName], [MinPrice], [MaxPrice], [Country]) VALUES (3, N'££££ (£41 and over)', 41.0000, 1000000.0000, N'UK')
SET IDENTITY_INSERT [dbo].[PriceRange] OFF
