-- Author:	Swati Agrawal
-- Summary:	Updated Report logic to give user an option to filter by dates

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.03.11.UpdateSearchReport'

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


UPDATE dbo.[Report]
SET Query = N'SELECT CASE 
         WHEN ( u.firstname IS NULL 
                AND u.lastname IS NULL ) THEN ''UNKNOWN'' 
         ELSE u.firstname + '' '' + u.lastname 
       END AS UserName, 
       keyword, 
       startdate, 
       enddate, 
       CASE 
         WHEN byob = 1 THEN ''true'' 
         ELSE ''false'' 
       END AS Byob, 
       CASE 
         WHEN charity = 1 THEN ''true'' 
         ELSE ''false'' 
       END AS Charity, 
       charity, 
       CASE 
         WHEN sourcepagetypeid = 1 THEN ''Search'' 
         ELSE ''Home'' 
       END AS Page, 
       CASE 
         WHEN cusine IS NULL THEN NULL 
         ELSE dbo.Getcusinesnamesfromids(Replace(cusine, '''''''', '''')) 
       END AS Cuisine, 
       CASE 
         WHEN diet IS NULL THEN NULL 
         ELSE dbo.Getdietnamesfromids(diet) 
       END AS Diet 
FROM   searchlogdetail s 
       LEFT JOIN [user] u 
              ON s.userid = u.id 
	   WHERE s.createddate BETWEEN @StartDate and @EndDate
ORDER  BY s.createddate DESC ', [parameters]='@StartDate=01-10-2016;@EndDate=31-12-2016'
WHERE Name= 'Search Report' AND ReportType=5

	
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
