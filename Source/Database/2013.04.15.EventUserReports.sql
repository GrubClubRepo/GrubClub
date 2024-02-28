-- Author:	Swati Agrawal
-- Summary:	This adds new reports in Event and Users Report types

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.04.15.EventUserReports'

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
	
INSERT INTO [SupperClub].[dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[Query]
           ,[ReportType])
     VALUES
           ('Event Master'
           ,'@EventId=1'
           ,'SELECT e.name as EventName, e.Start as EventDate, (u.FirstName + '' ''+ u.Lastname) as GuestName, a.Email as Email, baseprice as PricePerTicket, TotalPrice as PriceIncludingCommissionandVAT, baseprice as AmountDueToChef  FROM dbo.[Event] e WITH(NOLOCK)  INNER JOIN dbo.EventAttendee ea WITH(NOLOCK) ON e.Id= ea.EventId  INNER JOIN  dbo.[user] u WITH(NOLOCK) ON u.Id=ea.UserId  INNER JOIN  dbo.aspnet_Membership a WITH(NOLOCK) ON a.UserId=u.Id  INNER JOIN  dbo.Ticket t WITH(NOLOCK) ON t.eventid=ea.EventId and t.UserId = ea.UserId  WHERE e.Id=@EventId  ORDER BY e.Start ASC'
           ,1)
           

INSERT INTO [SupperClub].[dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[Query]
           ,[ReportType])
     VALUES
           ('Users List'
           , null
           ,'SELECT aspm.Email, u.FirstName, u.LastName, u.DateOfBirth, u.Gender, ISNULL(COUNT(DISTINCT ea.eventId),0) AS NoOfEventsAttended FROM dbo.[User] u INNER JOIN dbo.[aspnet_Membership] aspm ON u.Id = aspm.UserId LEFT JOIN dbo.[EventAttendee] ea ON ea.UserId = u.Id GROUP BY aspm.Email, u.FirstName, u.LastName, u.DateOfBirth, u.Gender'
           ,3)

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