-- Author:	Swati Agrawal
-- Summary:	This script creates EventWaitList table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.10.17.CreateEventWaitListTable'

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

CREATE TABLE [dbo].[EventWaitList](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[UserId] [uniqueidentifier] NULL,
	[AddedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
 CONSTRAINT [PK_EventWaitList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[EventWaitList]  WITH CHECK ADD  CONSTRAINT [FK_EventWaitList_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event]([Id])
ALTER TABLE [dbo].[EventWaitList] CHECK CONSTRAINT [FK_EventWaitList_Event]

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



