-- Author:	Swati Agrawal
-- Summary:	This adds the PushNotification related column to UserDevice table 

DECLARE @ScriptCode nvarchar(250)
SET @ScriptCode='2014.01.28.AddPushNotificationColumnToUserDeviceTable'

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

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserDevice'
AND COLUMN_NAME = 'FbFriendInstalledApp')
ALTER TABLE [dbo].[UserDevice] ADD FbFriendInstalledApp BIT NOT NULL DEFAULT(1)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserDevice'
AND COLUMN_NAME = 'FbFriendBookedTicket')
ALTER TABLE [dbo].[UserDevice] ADD FbFriendBookedTicket BIT NOT NULL DEFAULT(1)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserDevice'
AND COLUMN_NAME = 'FavChefNewEventNotification')
ALTER TABLE [dbo].[UserDevice] ADD FavChefNewEventNotification BIT NOT NULL DEFAULT(1)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserDevice'
AND COLUMN_NAME = 'FavEventBookingReminder')
ALTER TABLE [dbo].[UserDevice] ADD FavEventBookingReminder BIT NOT NULL DEFAULT(1)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserDevice'
AND COLUMN_NAME = 'WaitlistEventTicketsAvailable')
ALTER TABLE [dbo].[UserDevice] ADD WaitlistEventTicketsAvailable BIT NOT NULL DEFAULT(1)

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

