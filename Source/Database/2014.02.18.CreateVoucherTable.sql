-- Author:	Swati Agrawal
-- Summary:	This script creates Voucher table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.02.18.CreateVoucherTable'

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
CREATE TABLE [dbo].[Voucher](
[Id] [int] IDENTITY(1,1) NOT NULL,
[Code] [varchar](10) NOT NULL,
[Description] [varchar](64) NOT NULL,
[OwnerId] [int] NOT NULL,
[TypeId] [int] NOT NULL,
[OffValue] [int] NULL,
[TotalBooking] [int] NULL,
[FreeBooking] [int] NULL,
[SupperclubId] [int] NULL,
[IsGlobal] [Bit] NOT NULL,
[Active] [Bit] NOT NULL,
[ExpiryDate] [datetime] NULL,
[StartDate] [datetime] NULL,
[CreatedDate] [datetime] NOT NULL,
[MinBookingAmount] [decimal](22,4) NOT NULL DEFAULT(0),
[UsageCap] [int] NOT NULL DEFAULT(0),
[NumberOfTimesUsed] [int] NOT NULL DEFAULT(0),
[UniqueUserRedeemLimit] [int] NOT NULL DEFAULT(0),
 CONSTRAINT [PK_Voucher] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Voucher] ADD  CONSTRAINT [DF_Voucher_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
ALTER TABLE [dbo].[Voucher] ADD  CONSTRAINT [DF_Voucher_Active]  DEFAULT (1) FOR [Active]

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



