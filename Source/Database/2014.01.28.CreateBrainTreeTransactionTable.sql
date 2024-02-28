-- Author:	Swati Agrawal
-- Summary:	This script creates BrainTreeTransaction table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.28.CreateBrainTreeTransactionTable'

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
CREATE TABLE [dbo].[BrainTreeTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Amount] [decimal](18,4) NULL,
	[AvsErrorResponseCode] [varchar](50) NULL,
	[AvsPostalCodeResponseCode] [varchar](50) NULL,
	[AvsStreetAddressResponseCode] [varchar](50) NULL,
	[Channel] [varchar](50) NULL,
	[CvvResponseCode] [varchar](50) NULL,
	[TransactionCreationDate] [datetime] NULL,
	[TransactionUpdateDate] [datetime]  NULL,
	[TransactionId] [varchar](50) NULL,
	[MerchantAccountId] [varchar](50) NULL,
	[OrderId] [varchar](50) NULL,
	[PlanId] [varchar](50) NULL,
	[ProcessorAuthorizationCode] [varchar](50) NULL,
	[ProcessorResponseCode] [varchar](50) NULL,
	[ProcessorResponseText] [varchar](50) NULL,
	[PurchaseOrderNumber] [varchar](50) NULL,
	[SettlementBatchId] [varchar](50) NULL,
	[Status] [varchar](50) NULL,
	[TransactionType] [varchar](50) NULL,
	[ServiceFeeAmount] [decimal](18,4) NULL,
	[TaxAmount] [decimal](18,4) NULL,
	[TaxExempt] [bit] NULL,
	[TransactionProcessDate] [DateTime] NOT NULL,
	[TransactionStatus][varchar](128) NOT NULL,
    [TransactionSuccess][bit] NOT NULL,
    [TransactionValidVenmoSDK][bit] NOT NULL,
 CONSTRAINT [PK_BrainTreeTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[BrainTreeTransaction] ADD  CONSTRAINT [DF_BrainTreeTransaction_TransactionProcessDate]  DEFAULT (getdate()) FOR [TransactionProcessDate]

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



