-- Author:	Swati Agrawal
-- Summary:	This adds the CheckedIn and CheckInDate column to TicketBasket table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.14.AddNewColumnsToTicketBasketTable'

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


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TicketBasket'
AND COLUMN_NAME = 'CheckedIn')
ALTER TABLE [dbo].[TicketBasket] ADD CheckedIn BIT NOT NULL DEFAULT (0)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TicketBasket'
AND COLUMN_NAME = 'CheckInDate')
ALTER TABLE [dbo].[TicketBasket] ADD CheckInDate DATETIME NULL

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

