-- Author:	Swati Agrawal
-- Summary:	This adds report to view Sagepay transaction details

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.04.12.SagePayReport'

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
	
INSERT INTO [SupperClub].[dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[Query]
           ,[ReportType])
     VALUES
           ('SagePay Transaction Details'
           ,'@StartDate=2013-01-01;@EndDate=2013-12-31'
           ,'SELECT [Id], [PaymentStatus], [Payment3DStatus], [PaymentStatusDetail], [ResponseDate], [RequestDate], [VendorTxCode], [SecurityKey], [VpsTxId], [CAVV], [RedirectUrl], [DateInitialised] FROM [SupperClub].[dbo].[PaymentTransaction] WHERE RequestDate BETWEEN @StartDate AND @EndDate'
           ,6)          
   

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