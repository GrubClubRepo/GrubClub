-- Author:	Swati Agrawal
-- Summary:	This script creates EventVoucher table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.02.18.CreateEventVoucherTable'

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
CREATE TABLE [dbo].[EventVoucher](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[VoucherId] [int] NOT NULL,
 CONSTRAINT [PK_EventVoucher] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[EventVoucher]  WITH CHECK ADD  CONSTRAINT [FK_EventVoucher_Voucher] FOREIGN KEY([VoucherId])
REFERENCES [dbo].[Voucher] ([Id])
ALTER TABLE [dbo].[EventVoucher] CHECK CONSTRAINT [FK_EventVoucher_Voucher]

ALTER TABLE [dbo].[EventVoucher]  WITH CHECK ADD  CONSTRAINT [FK_EventVoucher_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event] ([Id])
ALTER TABLE [dbo].[EventVoucher] CHECK CONSTRAINT [FK_EventVoucher_Event]


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



