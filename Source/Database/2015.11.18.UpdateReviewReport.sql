-- Author:	Swati Agrawal
-- Summary:	added rating and private review info

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.18.UpdateReviewReport'

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
SET Query='SELECT CASE 
         WHEN r.userid IS NULL THEN '''' 
         ELSE u.firstname END AS FirstName, 
       CASE 
         WHEN r.userid IS NULL THEN '''' 
         ELSE u.lastname  END  AS LastName, 
       e.NAME         AS EventName, 
       r.datecreated  AS ReviewDate,
       r.Rating,
       r.publicreview AS PublicReview,
       r.PrivateReview
FROM   review r 
       INNER JOIN event e 
               ON r.eventid = e.id 
       LEFT JOIN [user] u 
              ON u.id = r.userid 
WHERE  publicreview IS NOT NULL 
       AND ( r.datecreated BETWEEN @StartDate AND @EndDate ) 
ORDER  BY r.datecreated DESC '
WHERE Name= 'Review Report' AND ReportType=5

	
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
