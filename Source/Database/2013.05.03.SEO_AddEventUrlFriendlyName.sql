-- Author:	Swati Agrawal
-- Summary:	SEO DB changes

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.05.03.SEO_AddEventUrlFriendlyName'

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

	EXEC sp_executesql N'CREATE FUNCTION [dbo].[RTrimChar]
	(
	    @input nvarchar(MAX),
	    @trimChar nchar(1)
	)
	RETURNS nvarchar(MAX)
	AS
	BEGIN
	DECLARE @len int, @index int;
	     
	    set @len = LEN(@input);
	    set @input = REVERSE(@input);
	    set @index = PATINDEX(''%[^'' + @trimChar + '']%'', @input);
	     
	    IF @index = 0
	        set @input = ''''
	    ELSE
	        set @input = REVERSE(SUBSTRING(@input, @index, @len));
	         
	    RETURN @input;
	END'
	
	EXEC sp_executesql N'CREATE FUNCTION [dbo].[GenerateSeoFriendlyUrl]
		(   
			@str VARCHAR(100)
		)
		RETURNS VARCHAR(100)
		AS
		BEGIN
		DECLARE @IncorrectCharLoc SMALLINT
		SET @str = LOWER(@str)
		SET @IncorrectCharLoc = PATINDEX(''%[^0-9a-z] %'',@str)
		WHILE @IncorrectCharLoc > 0
		BEGIN
		SET @str = STUFF(@str,@incorrectCharLoc,1,'''')
		SET @IncorrectCharLoc = PATINDEX(''%[^0-9a-z] %'',@str)
		END
		SET @str = REPLACE(@str,'' '',''-'')
		SET @str = REPLACE(@str,''&amp;'',''and'')
		SET @str = REPLACE(@str,''&'',''and'')
		SET @str = dbo.RTrimChar(@str, ''-'')
		RETURN @str
		END'
		
		ALTER TABLE [Event]
		ADD UrlFriendlyName NVARCHAR(500) NULL

		EXEC sp_executesql N'UPDATE [Event] SET UrlFriendlyName = s.UrlFriendlyName + ''-''+ dbo.GenerateSeoFriendlyUrl(e.name)
			FROM dbo.[Event] e with(nolock) 
			INNER JOIN dbo.[SupperClub] s with(nolock) ON e.SupperClubId = s.Id'
			
		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('notes-from-a-pop-up-restaurant'
				   ,'NotesFromASupperClub')

			
		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
			   ([RewriteUrl]
			   ,[ActualUrl])
			VALUES
			   ('contact-us'
			   ,'ContactUs')
	

		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('about-us'
				   ,'AboutUs')


		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('how-it-works-guests'
				   ,'HowItWorksGuests')


		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('how-it-works-hosts'
				   ,'HowItWorksHosts')


		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('faq-guest'
				   ,'FAQsGuest')


		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('faq-host'
				   ,'FAQsHost')


		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('how-to-host'
				   ,'HowToHost')


		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('what-is-a-grub-club'
				   ,'WhatIsAGrubClub')

		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('terms-and-conditions'
				   ,'TermsAndConditions')

		INSERT INTO [SupperClub].[dbo].[UrlRewrite]
				   ([RewriteUrl]
				   ,[ActualUrl])
			 VALUES
				   ('site-map'
				   ,'SiteMap')
		

		
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


