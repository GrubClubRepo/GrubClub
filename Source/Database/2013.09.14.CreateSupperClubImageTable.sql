-- Author:	Swati Agrawal
-- Summary:	This script creates SupperClubImage table 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.09.14.CreateSupperClubImageTable'

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

CREATE TABLE [dbo].[SupperClubImage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SupperClubId] [int] NOT NULL,
	[ImagePath] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SupperClubImage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SupperClubImage]  WITH CHECK ADD  CONSTRAINT [FK_SupperClubImage_SupperClub] FOREIGN KEY([SupperClubId])
REFERENCES [dbo].[SupperClub] ([Id])
ALTER TABLE [dbo].[SupperClubImage] CHECK CONSTRAINT [FK_SupperClubImage_SupperClub]


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

