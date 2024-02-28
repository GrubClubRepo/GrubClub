-- Author:	Swati Agrawal
-- Summary:	This script creates UserFacebookFriend table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.28.CreateUserFacebookFriendTable'

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

CREATE TABLE [dbo].[UserFacebookFriend](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[UserId] [uniqueidentifier] NOT NULL,
	[UserFacebookId] [varchar](50) NOT NULL,
	[FriendFacebookId] [varchar](50) NOT NULL,		
 CONSTRAINT [PK_UserFacebookFriend] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[UserFacebookFriend]  WITH CHECK ADD  CONSTRAINT [FK_UserFacebookFriend_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ALTER TABLE [dbo].[UserFacebookFriend] CHECK CONSTRAINT [FK_UserFacebookFriend_User]
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



