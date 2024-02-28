-- Author:	Swati Agrawal
-- Summary:	This script creates UserFavouriteEvent table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.28.CreateUserFavouriteEventTable'

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

CREATE TABLE [dbo].[UserFavouriteEvent](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[UserId] [uniqueidentifier] NOT NULL,
	[EventId] [int] NOT NULL,		
	[NotificationSent] [bit] NOT NULL DEFAULT(0),
 CONSTRAINT [PK_UserFavouriteEvent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[UserFavouriteEvent]  WITH CHECK ADD  CONSTRAINT [FK_UserFavouriteEvent_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event] ([Id])
ALTER TABLE [dbo].[UserFavouriteEvent] CHECK CONSTRAINT [FK_UserFavouriteEvent_Event]

ALTER TABLE [dbo].[UserFavouriteEvent]  WITH CHECK ADD  CONSTRAINT [FK_UserFavouriteEvent_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ALTER TABLE [dbo].[UserFavouriteEvent] CHECK CONSTRAINT [FK_UserFavouriteEvent_User]

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



