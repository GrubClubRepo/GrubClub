-- Author:	Swati Agrawal
-- Summary:	Updated new 

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.05.13.UpdateSeoUrls'

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


UPDATE dbo.UrlRewrite
SET RewriteUrl='how-to-find-and-book-a-grub-club'
WHERE ActualUrl= 'HowItWorksGuests'

UPDATE dbo.UrlRewrite
SET RewriteUrl='how-to-set-up-a-grub-club'
WHERE ActualUrl= 'HowItWorksHosts'

UPDATE dbo.UrlRewrite
SET RewriteUrl='what-you-need-to-know-about-attending-a-grub-club'
WHERE ActualUrl= 'FAQsGuest'

UPDATE dbo.UrlRewrite
SET RewriteUrl='how-to-set-up-a-pop-up-restaurant'
WHERE ActualUrl= 'FAQsHost'

UPDATE dbo.UrlRewrite
SET RewriteUrl='how-to-set-up-a-supperclub'
WHERE ActualUrl= 'HowToHost'

	
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
