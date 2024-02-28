-- Author:	Swati Agrawal
-- Summary:	This script creates FavouriteEvent table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.01.13.CreateFavouriteEventTable'

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

CREATE TABLE [dbo].[FavouriteEvent](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
 CONSTRAINT [PK_FavouriteEvent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[FavouriteEvent]  WITH CHECK ADD  CONSTRAINT [FK_FavouriteEvent_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event] ([Id])
ALTER TABLE [dbo].[FavouriteEvent] CHECK CONSTRAINT [FK_FavouriteEvent_Event]

EXEC sp_executesql N'INSERT INTO [FavouriteEvent](EventId) VALUES (747)'

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



