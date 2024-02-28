-- Author:	Swati Agrawal
-- Summary:	This updates the Email Template for guest booking confirmation email

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.05.07.AddColumnToDietTable'

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

--BookingConfirmedGuest = 4,
ALTER TABLE Diet 
ADD DietTypeId [int] NULL 

EXEC sp_executesql N'UPDATE Diet SET DietTypeId = 1

INSERT INTO Diet(Name, DietTypeId) VALUES (''Nuts'',2)

INSERT INTO Diet(Name, DietTypeId) VALUES (''Dairy'',2)

INSERT INTO Diet(Name, DietTypeId) VALUES (''Wheat'',2)

INSERT INTO Diet(Name, DietTypeId) VALUES (''Shellfish'',2)

ALTER TABLE [Diet] ALTER COLUMN DietTypeId INT NOT NULL

ALTER TABLE [Diet] ADD CONSTRAINT DF_Diet_DietTypeId default 3 FOR DietTypeId'


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