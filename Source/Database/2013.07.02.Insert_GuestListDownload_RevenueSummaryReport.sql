-- Author:	Swati Agrawal
-- Summary:	This inserts query scripts for GuestListDownload & RevenueSummaryReport

DECLARE @ScriptCode nvarchar(250)
SET @ScriptCode='2013.07.02.Insert_GuestListDownload_RevenueSummaryReport'

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
           ('Host Guest List Download'
           ,'@EventId=1'
           ,1
           ,N'SELECT DISTINCT CASE WHEN ea.SeatingId = 0 THEN e.Start ELSE es.Start END AS [EventTime],  u.FirstName + '' '' + u.LastName as [Name],  
  am.Email, ea.NumberOfGuests as NumberOfTickets, ISNULL(tb.BookingRequirements,'''') AS BookingRequirements, CASE WHEN ea.MenuOptionId = 0 THEN '''' ELSE emo.Title END as MenuOptionChosen
   FROM [Event] e 
   INNER JOIN EventAttendee ea on ea.EventId = e.Id  
   INNER JOIN [User] u on u.Id = ea.UserId   
   INNER JOIN [aspnet_Membership] am  on am.UserId = u.Id  
   LEFT JOIN [Ticket] t on t.EventId = ea.EventId AND t.SeatingId = ea.SeatingId AND t.MenuOptionId = ea.MenuOptionId AND t.UserId = ea.UserId
   INNER JOIN [TicketBasket] tb on tb.Id = t.BasketId
   LEFT JOIN [EventSeating] es ON es.Id = ea.SeatingId
   LEFT JOIN [EventMenuOption] emo ON emo.Id = ea.MenuOptionId
   WHERE e.Id = @EventId  
   ORDER BY 1')
   
INSERT INTO [dbo].[Report]
           ([Name]
           ,[Parameters]
           ,[ReportType]
           ,[Query]
           )
     VALUES
           ('Revenue Summary Report'
           ,'@StartDate=01-01-2013;@EndDate=31-12-2013'
           ,4
           ,N'
CREATE TABLE #Stats1(Start DATETIME, Guests INT, ReservedSeats INT, NumberOfGuests INT, AmountToChef DECIMAL(22,2), GCcommission DECIMAL(22,2), TotalSales DECIMAL(22,2))
CREATE TABLE #Stats2(Start DATETIME, Guests INT, ReservedSeats INT, NumberOfGuests INT, AmountToChef DECIMAL(22,2), GCcommission DECIMAL(22,2), TotalSales DECIMAL(22,2))

IF @StartDate < ''13-05-2013'' 
BEGIN
INSERT INTO #Stats1 SELECT Start, SUM(Guests) AS Guests, SUM(ReservedSeats)  AS ReservedSeats, ISNULL(SUM(NumberOfGuests), 0) AS NumberOfGuests,
				SUM(AmountToChef) AS AmountToChef, SUM(AmountToChef)* 0.04 AS GCcommission, SUM(AmountToChef)*1.04 AS TotalSales 
         FROM   Event E 
                LEFT JOIN (SELECT EventId,Cost,SUM(NumberOfGuests)AS NumberOfGuests,Cost * SUM(Numberofguests) AS AmountToChef 
								FROM   dbo.[Eventattendee] EA 
                                INNER JOIN dbo.[Event] E ON E.Id = EA.Eventid 
								WHERE  Active = 1 AND (Start >= @StartDate AND Start < (CASE WHEN @EndDate < ''13-05-2013'' THEN DATEADD(dd,1,@EndDate) ELSE ''13-05-2013'' END))
								GROUP  BY Eventid,Cost)B 
                ON E.Id = B.Eventid 
         WHERE  Active = 1 AND (Start >= @StartDate AND Start < (CASE WHEN @EndDate < ''13-05-2013'' THEN DATEADD(dd,1,@EndDate) ELSE ''13-05-2013'' END))
         GROUP  BY Start         
END

IF @EndDate >= ''13-05-2013'' 
BEGIN
INSERT INTO #Stats2 SELECT Start, SUM(Guests) AS Guests, SUM(ReservedSeats)  AS ReservedSeats, ISNULL(SUM(NumberOfGuests), 0) AS NumberOfGuests,
				SUM(AmountToChef) AS AmountToChef, SUM(AmountToChef)* 0.1 AS GCcommission, SUM(AmountToChef)*1.1 AS TotalSales 
         FROM   Event E 
                LEFT JOIN (SELECT EventId,Cost,SUM(NumberOfGuests)AS NumberOfGuests,Cost * SUM(Numberofguests) AS AmountToChef 
								FROM   dbo.[Eventattendee] EA 
                                INNER JOIN dbo.[Event] E ON E.Id = EA.Eventid 
								WHERE  Active = 1 AND (Start >= (CASE WHEN @StartDate >= ''13-05-2013'' THEN @StartDate ELSE ''13-05-2013'' END) AND Start <= DATEADD(dd,1,@EndDate))
								GROUP  BY Eventid,Cost)B 
                ON E.Id = B.Eventid 
         WHERE  Active = 1 AND (Start > (CASE WHEN @StartDate >= ''13-05-2013'' THEN @StartDate ELSE ''13-05-2013'' END) AND Start <= DATEADD(dd,1,@EndDate))
         GROUP  BY Start         
END

SELECT DATENAME( MONTH , DATEADD( MONTH , MONTH(Start) , 0 ) - 1 ) AS [Month], 
       ISNULL(SUM(Guests),0) AS Guests, 
       ISNULL(SUM(ReservedSeats),0) AS ReservedSeats, 
       ISNULL(SUM(NumberOfGuests),0) AS NumberOfBookings, 
       ISNULL(SUM(AmountToChef),0) AS AmountToChef, 
       ISNULL(SUM(GCcommission),0) AS GCcommission, 
       ISNULL(SUM(TotalSales),0) AS TotalSales
FROM   (SELECT Start, Guests, ReservedSeats, NumberOfGuests, AmountToChef, GCcommission, TotalSales FROM #Stats1 
        UNION ALL 
        SELECT Start, Guests, ReservedSeats, NumberOfGuests, AmountToChef, GCcommission, TotalSales FROM #Stats2)A 
GROUP  BY YEAR(Start), MONTH(Start) 
ORDER  BY YEAR(Start) 

DROP TABLE #Stats1
DROP TABLE #Stats2')



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


