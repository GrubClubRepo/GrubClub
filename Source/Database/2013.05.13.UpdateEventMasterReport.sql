-- Author:	Swati Agrawal
-- Summary:	Updated Commission percentage

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.05.13.UpdateEventMasterReport'

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
SET Query='SELECT e.name as EventName, e.Start as EventDate, a.Email as Email,u.FirstName, u.Lastname, (u.FirstName + '' ''+ u.Lastname) as GuestName, Cost as PricePerTicket, CAST((ROUND(Cost + (Cost*0.10/1.20)+(Cost*0.10*0.2/1.20),2)) AS DECIMAL(36,2)) as CostToGuestPerTicket, NumberOfGuests, CAST((NumberOfGuests * ROUND(Cost + (Cost*0.10/1.20)+(Cost*0.10*0.2/1.20),2)) AS DECIMAL(36,2)) as PriceIncludingCommissionandVAT, NumberOfGuests * cost as AmountDueToChef FROM dbo.[Event] e WITH(NOLOCK) INNER JOIN dbo.EventAttendee ea WITH(NOLOCK) ON e.Id= ea.EventId INNER JOIN  dbo.[user] u WITH(NOLOCK) ON u.Id=ea.UserId INNER JOIN  dbo.aspnet_Membership a WITH(NOLOCK) ON a.UserId=u.Id WHERE e.Id=@EventId ORDER BY e.Start ASC'
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
