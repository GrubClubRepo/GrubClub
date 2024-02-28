-- Author:	Swati Agrawal
-- Summary:	This adds the MaxTicketsAllowed and MinTicketsAllowed column to Event table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.30.AddColumnsToEventTable'

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

