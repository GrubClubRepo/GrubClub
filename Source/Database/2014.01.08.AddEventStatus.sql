-- Author:	Swati Agrawal
-- Summary:	This adds Status for an event

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.08.AddEventStatus'

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
	ADD [Status] INT NULL

	EXEC sp_executesql N'
		UPDATE [Event] SET [Status] = (CASE WHEN Active = 1 THEN 1 ELSE 2 END)
		ALTER TABLE [Event]
		ALTER COLUMN [Status] INT NOT NULL	
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