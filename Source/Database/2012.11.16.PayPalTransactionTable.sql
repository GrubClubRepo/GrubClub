-- Author:	Josh Helmink
-- Summary:	New PayPal table

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2012.11.16.PayPalTransactionTable'

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

CREATE TABLE [dbo].[PayPalTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TrackingReference] [nvarchar](50) NOT NULL,
	[RequestId] [nvarchar](50) NOT NULL,
	[RequestTime] [datetime] NOT NULL,
	[RequestStatus] [nvarchar](50) NOT NULL,
	[TimeStamp] [nvarchar](50) NOT NULL,
	[RequestError] [nvarchar](max) NULL,
	[Token] [nvarchar](50) NOT NULL,
	[PayerId] [nvarchar](50) NULL,
	[RequestData] [nvarchar](max) NULL,
	[PaymentTransactionId] [nvarchar](50) NULL,
	[PaymentError] [nvarchar](max) NULL
) ON [PRIMARY]

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