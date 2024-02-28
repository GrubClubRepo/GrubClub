-- Author:	Swati Agrawal
-- Summary:	This script creates TileTag table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.08.06.CreateTileTagTable'

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

CREATE TABLE [dbo].[TileTag](
	[TileId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
	[ImageId] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_TileTag] PRIMARY KEY CLUSTERED 
(
	[TileId] ASC, [TagId] ASC, [ImageId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[TileTag]  WITH CHECK ADD  CONSTRAINT [FK_TileTag_Tag] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tag] ([Id])
ALTER TABLE [dbo].[TileTag] CHECK CONSTRAINT [FK_TileTag_Tag]

ALTER TABLE [dbo].[TileTag]  WITH CHECK ADD  CONSTRAINT [FK_TileTag_Tile] FOREIGN KEY([TileId])
REFERENCES [dbo].[Tile] ([Id])
ALTER TABLE [dbo].[TileTag] CHECK CONSTRAINT [FK_TileTag_Tile]

ALTER TABLE [dbo].[TileTag]  WITH CHECK ADD  CONSTRAINT [FK_TileTag_Image] FOREIGN KEY([ImageId])
REFERENCES [dbo].[Image] ([Id])
ALTER TABLE [dbo].[TileTag] CHECK CONSTRAINT [FK_TileTag_Image]

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
