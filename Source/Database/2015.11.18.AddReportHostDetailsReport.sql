-- Author:	Swati Agrawal
-- Summary:	This returns all chef's details

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.18.AddReportHostDetailsReport'

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
           ('Host Details Report'
           ,NULL
           ,2
           ,N'SELECT u.firstname AS ChefFirstName,
u.LastName as ChefLastName,
u.ContactNumber AS ContactNumber,
asp.Email,
S.NAME       AS ProfileName, 
       TotalEventsHosted, 
       FirstEventDate,
       LastEventDate,
       AverageRating,
       NumberOfReviews,
       (S.Address + case when address2 is null then '''' else '',''+address2 end) AS Address,
       s.City,
       s.PostCode,
       s.Country     
FROM   dbo.supperclub S WITH(nolock) 
       INNER JOIN (SELECT supperclubid,                           
                          COUNT(id) AS TotalEventsHosted,
                          min(Start) AS FirstEventDate,
                          max(Start) AS LastEventDate                             
                   FROM   dbo.event E WITH(nolock) 
                   where Status in(1,3)
                   GROUP  BY supperclubid)EC 
               ON EC.supperclubid = S.id        
       INNER JOIN dbo.[User] u WITH(nolock)
               ON u.id = s.userid
       INNER JOIN dbo.aspnet_membership asp WITH(nolock) 
               ON u.id = asp.userid
        LEFT JOIN (SELECT SupperClubId, 
                         Count(r.id) AS NumberOfReviews,
                         Avg(rating) AS AverageRating
                  FROM   dbo.Review r WITH(nolock) 
                  inner join dbo.[event] e WITH(nolock) on r.eventid = e.id
                  GROUP  BY SupperClubId)ER 
              ON s.id = ER.SupperClubId 
           ')
   
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




