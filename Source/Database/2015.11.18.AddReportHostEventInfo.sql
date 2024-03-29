-- Author:	Swati Agrawal
-- Summary:	This returns chef and it's events related info

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.18.AddReportHostEventInfo'

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
           ('Host Event Info Report'
           ,NULL
           ,2
           ,N'select S.Name AS ProfileName,
   TotalEvents,
   EventApproved,
   EventRejected,
   E.Name AS EventName,
   case when E.Status =1 then ''Approved'' when e.Status=4 then ''Rejected'' when e.Status=3 then ''New'' else ''Cancelled'' end AS EventStatus,
   E.Cost * 1.1 AS PriceAdvertised,
   case when E.MultiSeating =1 and ES.EventId is not null then ES.Guests ELSE E.Guests end AS NumberOfPeopleCookedFor
   from dbo.SupperClub S with(nolock)
   inner join (SELECT SupperClubId, count(distinct E.Id) AS TotalEvents,
   SUM(case when E.Status = 1 then 1 else 0 end) AS EventApproved,
   SUM(case when E.Status = 4 then 1 else 0 end) AS EventRejected
   
   FROM dbo.Event E with(nolock) GROUP by SupperClubId)EC on EC.SupperClubId = S.Id
   inner join dbo.Event E with(nolock) on S.Id = E.SupperClubId   
   left join (SELECT EventId, SUM(Guests) AS Guests from dbo.EventSeating with(nolock) group by EventId)ES on E.Id = ES.EventId')
   
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




