-- Author:	Swati Agrawal
-- Summary:	This script creates EventPriceChangeLo table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.05.05.CreateEventPriceChangeLogTable'

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
CREATE TABLE [dbo].[EventPriceChangeLog](
[Id] [int] IDENTITY(1,1) NOT NULL,
[EventId] [int] NOT NULL,
[MenuOptionId] [int] NOT NULL,
[UserId] [uniqueidentifier] NOT NULL,
[OldPrice] [money] NOT NULL,
[NewPrice] [money] NOT NULL,
[Date] [datetime] NOT NULL,
 CONSTRAINT [PK_EventPriceChangeLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[EventPriceChangeLog] ADD  CONSTRAINT [DF_EventPriceChangeLog_Date]  DEFAULT (getdate()) FOR [Date]

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



