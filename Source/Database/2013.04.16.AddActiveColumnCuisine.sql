-- Author:	Swati Agrawal
-- Summary:	This adds a new column "Active" to Cuisine table

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.04.16.AddActiveColumnCuisine'

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
ALTER TABLE dbo.[Cuisine]
ADD Active BIT NULL

EXEC sp_executesql N'
		UPDATE [Cuisine] SET Active = 1
		
		UPDATE dbo.[Cuisine]
        SET Active = 0
        WHERE ID IN (70,71,64,79,75,35,82,83,52,88,92,66,55,56,67,72,81,68,69,74,61,59,62,78,93,91,94,34,65)
        
		ALTER TABLE [Cuisine]
		ALTER COLUMN Active BIT NOT NULL
		
		ALTER TABLE [Cuisine]
            ADD CONSTRAINT DF_Cuisine_Active
            DEFAULT ''1'' FOR Active
            
       UPDATE dbo.[Cuisine] SET Name = ''American'' WHERE Id=70 
       UPDATE dbo.[Cuisine] SET Name = ''European'' WHERE Id=71 AND Name LIKE ''Artisan Bread''
UPDATE dbo.[Cuisine] SET Name = ''European'' WHERE Id=64 AND Name LIKE ''Beef''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=79 AND Name LIKE ''British Seafood''
UPDATE dbo.[Cuisine] SET Name = ''European'' WHERE Id=75 AND Name LIKE ''Brunch''
UPDATE dbo.[Cuisine] SET Name = ''European'' WHERE Id=35 AND Name LIKE ''Continental''
UPDATE dbo.[Cuisine] SET Name = ''English'' WHERE Id=82 AND Name LIKE ''EnglishEnglish''
UPDATE dbo.[Cuisine] SET Name = ''English'' WHERE Id=83 AND Name LIKE ''EnglishEnglishEnglish''
UPDATE dbo.[Cuisine] SET Name = ''Caribbean'' WHERE Id=52 AND Name LIKE ''Fine Creole''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=88 AND Name LIKE ''Food=British; Nutrition=Perfect''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=92 AND Name LIKE ''Foraged & Wild Medieval Banquet''
UPDATE dbo.[Cuisine] SET Name = ''European'' WHERE Id=66 AND Name LIKE ''Gourmet Vegan''
UPDATE dbo.[Cuisine] SET Name = ''European'' WHERE Id=55 AND Name LIKE ''Improvised vegetarian food!''
UPDATE dbo.[Cuisine] SET Name = ''Italian'' WHERE Id=56 AND Name LIKE ''Italian pizza al taglio''
UPDATE dbo.[Cuisine] SET Name = ''Italian'' WHERE Id=67 AND Name LIKE ''ItalianItalian''
UPDATE dbo.[Cuisine] SET Name = ''Italian'' WHERE Id=72 AND Name LIKE ''ItalianItalianItalian''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=81 AND Name LIKE ''molecular''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=68 AND Name LIKE ''Mussels''
UPDATE dbo.[Cuisine] SET Name = ''American'' WHERE Id=69 AND Name LIKE ''Novel inspired''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=74 AND Name LIKE ''Oysters and Beer''
UPDATE dbo.[Cuisine] SET Name = ''British'' WHERE Id=61 AND Name LIKE ''Raw''
UPDATE dbo.[Cuisine] SET Name = ''Russian'' WHERE Id=59 AND Name LIKE ''RussianRussian''
UPDATE dbo.[Cuisine] SET Name = ''Russian'' WHERE Id=62 AND Name LIKE ''RussianRussianRussian''
UPDATE dbo.[Cuisine] SET Name = ''Russian'' WHERE Id=78 AND Name LIKE ''RussianRussianRussianRussianRussian''
UPDATE dbo.[Cuisine] SET Name = ''Mediterranean'' WHERE Id=93 AND Name LIKE ''Rustic Mediterranean Italian''
UPDATE dbo.[Cuisine] SET Name = ''Chinese'' WHERE Id=91 AND Name LIKE ''Shandong Province''
UPDATE dbo.[Cuisine] SET Name = ''Caribbean'' WHERE Id=94 AND Name LIKE ''Soul Food''
UPDATE dbo.[Cuisine] SET Name = ''English'' WHERE Id=34 AND Name LIKE ''Traditional English''
UPDATE dbo.[Cuisine] SET Name = ''Eurasian'' WHERE Id=65 AND Name LIKE ''Unique blend of Eurasian (is that a word?)'''

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