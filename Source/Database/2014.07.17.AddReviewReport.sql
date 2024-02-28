-- Author:	Swati Agrawal
-- Summary:	This returns reviews for all events during a defined time period

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.07.17.AddReviewReport'

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

INSERT INTO [dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[ReportType]
           ,[Query]
           )
     VALUES
           ('Review Report'
           ,'@StartDate=01-01-2012;@EndDate=31-12-2015'
           ,5
           ,N'select case when r.userid is null then '''' else u.firstname + '' '' +u.lastname end as GuestName,  e.name AS GrubClub,   r.DateCreated AS ReviewDate,   r.publicreview AS PublicReview  from review r  inner join event e on r.eventid = e.id  left join [user] u on u.id = r.userid  where publicreview is not null  and (r.Datecreated between @StartDate and @EndDate)  order by r.datecreated desc')
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


