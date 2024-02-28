-- Author:	Swati Agrawal
-- Summary:	This adds VoucherId and Discount columns to Ticket table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.02.18.AddColumnsToTicketTable'

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


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Ticket'
AND COLUMN_NAME = 'VoucherId')
ALTER TABLE [dbo].[Ticket] ADD VoucherId INT NOT NULL DEFAULT(0)

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Ticket'
AND COLUMN_NAME = 'DiscountAmount')
ALTER TABLE [dbo].[Ticket] ADD DiscountAmount DECIMAL(18,4) NOT NULL DEFAULT(0)

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

