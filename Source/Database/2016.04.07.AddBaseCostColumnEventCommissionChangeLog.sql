-- Author:	Swati Agrawal
-- Summary:	This adds two new columns "OldCost" and "NewCost" to EventCommissionChangeLog table

DECLARE @ScriptCode nvarchar(100)
SET @ScriptCode='2016.04.07.AddCostColumnsEventCommissionChangeLog'

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
ALTER TABLE [dbo].[EventCommissionChangeLog]
ADD OldCost Money NULL

EXEC sp_executesql N'
		UPDATE [dbo].[EventCommissionChangeLog] SET OldCost=e.cost from Event e inner join [dbo].[EventCommissionChangeLog] ec on e.id=ec.eventid
		
		ALTER TABLE [dbo].[EventCommissionChangeLog]
		ALTER COLUMN OldCost Money NOT NULL'

		ALTER TABLE [dbo].[EventCommissionChangeLog]
		ADD NewCost Money NULL

		EXEC sp_executesql N'
		UPDATE [dbo].[EventCommissionChangeLog] SET NewCost=e.cost from Event e inner join [dbo].[EventCommissionChangeLog] ec on e.id=ec.eventid
		
		ALTER TABLE [dbo].[EventCommissionChangeLog]
		ALTER COLUMN NewCost Money NOT NULL'

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