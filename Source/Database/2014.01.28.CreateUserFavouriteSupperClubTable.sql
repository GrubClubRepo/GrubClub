-- Author:	Swati Agrawal
-- Summary:	This script creates UserFavouriteSupperClub table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.28.CreateUserFavouriteSupperClubTable'

PRINT '-------------------------------------------------------------------------------'
PRINT @ScriptCode
PRINT '-------------------------------------------------------------------------------'

IF(EXISTS(SELECT * FROM dbo.ScriptUpdate WHERE ScriptCode=@ScriptCode))
  PRINT N'Script was executed before'
ELSE
  BEGIN TRY
    BEGIN TRANSACTION

    set ANSI_NULLS ON
    set QUOTED_IDENTIFIER ON
    
----SCRIPT GOES BELOW THIS LINE--------------------------------------------------------

CREATE TABLE [dbo].[UserFavouriteSupperClub](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[UserId] [uniqueidentifier] NOT NULL,
	[SupperClubId] [int] NOT NULL,		
 CONSTRAINT [PK_UserFavouriteSupperClub] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[UserFavouriteSupperClub]  WITH CHECK ADD  CONSTRAINT [FK_UserFavouriteSupperClub_SupperClub] FOREIGN KEY([SupperClubId])
REFERENCES [dbo].[SupperClub] ([Id])
ALTER TABLE [dbo].[UserFavouriteSupperClub] CHECK CONSTRAINT [FK_UserFavouriteSupperClub_SupperClub]

ALTER TABLE [dbo].[UserFavouriteSupperClub]  WITH CHECK ADD  CONSTRAINT [FK_UserFavouriteSupperClub_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ALTER TABLE [dbo].[UserFavouriteSupperClub] CHECK CONSTRAINT [FK_UserFavouriteSupperClub_User]
----SCRIPT GOES ABOVE THIS LINE--------------------------------------------------------
    INSERT INTO dbo.ScriptUpdate(ScriptCode) VALUES(@ScriptCode)
  
    COMMIT
    PRINT N'Script executed'
  END TRY
  BEGIN CATCH
    ROLLBACK
    PRINT ERROR_MESSAGE()
    RAISERROR(N'Rolling back script.',11,1)
  END CATCH
PRINT '-------------------------------------------------------------------------------'
PRINT ''



