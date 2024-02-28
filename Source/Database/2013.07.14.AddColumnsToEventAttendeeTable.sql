-- Author:	Swati Agrawal
-- Summary:	This adds the columns to EventAttendee table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.07.14.AddColumnsToEventAttendeeTable'

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

ALTER TABLE dbo.EventAttendee ADD
	TotalBasePrice decimal(18, 4) NULL,
	TotalPrice decimal(18, 4) NULL,
	BookingDate datetime NULL DEFAULT GETDATE()

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

