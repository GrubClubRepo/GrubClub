-- Author:	Josh Helmink
-- Summary:	This add reserved seating to an event

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2012.12.06.AddReservedSeats'

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

	ALTER TABLE [Event]
	ADD ReservedSeats INT NULL

	EXEC sp_executesql N'
		UPDATE [Event] SET ReservedSeats = 0
		
		ALTER TABLE [Event]
		ALTER COLUMN ReservedSeats INT NOT NULL	
	'

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