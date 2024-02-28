-- Author:	Swati Agrawal
-- Summary:	Updated Report logic to show one user's bookings for one seating only once

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.07.18.UpdateEventMasterReport'

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
SET Query = N'SELECT e.name as EventName, 
e.Start as EventDate, 
a.Email as Email,
u.FirstName, u.Lastname, (u.FirstName + '' ''+ u.Lastname) as GuestName, 
CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END as PricePerTicket, 
CAST((ROUND(CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END + (CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END*0.10/1.20)+(CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END*0.10*0.2/1.20),2)) AS DECIMAL(36,2)) as CostToGuestPerTicket, 
NumberOfGuests, 
CAST((NumberOfGuests * ROUND(CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END + (CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END*0.10/1.20)+(CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END*0.10*0.2/1.20),2)) AS DECIMAL(36,2)) as PriceIncludingCommissionandVAT, 
NumberOfGuests * CASE WHEN e.MultiMenuOption =1 THEN emo.Cost ELSE e.Cost END as AmountDueToChef, 
isnull(emo.Title, '''') as MenuOption 
FROM dbo.[Event] e WITH(NOLOCK) 
INNER JOIN (SELECT EventId, UserId, MenuOptionId, SUM(NumberOfGuests) AS NumberOfGuests FROM dbo.EventAttendee  WITH(NOLOCK) WHERE EventId=@EventId GROUP BY EventId, UserId, MenuOptionId)ea ON e.Id= ea.EventId 
INNER JOIN  dbo.[user] u WITH(NOLOCK) ON u.Id=ea.UserId 
INNER JOIN  dbo.aspnet_Membership a WITH(NOLOCK) ON a.UserId=u.Id LEFT JOIN [EventMenuOption] emo ON emo.Id = ea.MenuOptionId WHERE e.Id=@EventId ORDER BY firstname ASC'
WHERE Name= 'Event Master' AND [Parameters]='@EventId=1' AND ReportType=1

	
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
