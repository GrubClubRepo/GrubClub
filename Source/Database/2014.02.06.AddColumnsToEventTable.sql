-- Author:	Swati Agrawal
-- Summary:	This adds new columns to Event table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.02.06.AddColumnsToEventTable'

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


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'MaxTicketsAllowed')
ALTER TABLE [dbo].[Event] ADD MaxTicketsAllowed INT NOT NULL DEFAULT(30)

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'MinTicketsAllowed')
ALTER TABLE [dbo].[Event] ADD MinTicketsAllowed INT NOT NULL DEFAULT(1)

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'ToggleMenuSelection')
ALTER TABLE [dbo].[Event] ADD ToggleMenuSelection BIT NOT NULL DEFAULT(0)

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'MinMaxBookingEnabled')
ALTER TABLE [dbo].[Event] ADD MinMaxBookingEnabled BIT NOT NULL DEFAULT(0)

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Event'
AND COLUMN_NAME = 'SeatSelectionInMultipleOfMin')
ALTER TABLE [dbo].[Event] ADD SeatSelectionInMultipleOfMin BIT NOT NULL DEFAULT(0)

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

