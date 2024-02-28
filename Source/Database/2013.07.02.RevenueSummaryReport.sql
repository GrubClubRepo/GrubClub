-- Author:	Swati Agrawal
-- Summary:	This adds new reports in Revenue types

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.07.02.RevenueSummaryReport'

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
           ('Revenue Summary Report'
           ,'@StartDate=01-01-2013;@EndDate=31-12-2013'
           ,4
           ,N'
CREATE TABLE #Stats1(Start DATETIME, Guests INT, ReservedSeats INT, NumberOfGuests INT, AmountToChef DECIMAL(22,2), GCcommission DECIMAL(22,2), TotalSales DECIMAL(22,2))
CREATE TABLE #Stats2(Start DATETIME, Guests INT, ReservedSeats INT, NumberOfGuests INT, AmountToChef DECIMAL(22,2), GCcommission DECIMAL(22,2), TotalSales DECIMAL(22,2))

IF @StartDate < ''13-may-2013'' 
BEGIN
INSERT INTO #Stats1 SELECT Start, SUM(Guests) AS Guests, SUM(ReservedSeats)  AS ReservedSeats, ISNULL(SUM(NumberOfGuests), 0) AS NumberOfGuests,
				SUM(AmountToChef) AS AmountToChef, SUM(AmountToChef)* 0.04 AS GCcommission, SUM(AmountToChef)*1.04 AS TotalSales 
         FROM   Event E 
                LEFT JOIN (SELECT EventId,Cost,SUM(NumberOfGuests)AS NumberOfGuests,Cost * SUM(Numberofguests) AS AmountToChef 
								FROM   dbo.[Eventattendee] EA 
                                INNER JOIN dbo.[Event] E ON E.Id = EA.Eventid 
								WHERE  Active = 1 AND (Start >= @StartDate AND Start < (CASE WHEN @EndDate < ''13-may-2013'' THEN DATEADD(dd,1,@EndDate) ELSE ''13-may-2013'' END))
								GROUP  BY Eventid,Cost)B 
                ON E.Id = B.Eventid 
         WHERE  Active = 1 AND (Start >= @StartDate AND Start < (CASE WHEN @EndDate < ''13-may-2013'' THEN DATEADD(dd,1,@EndDate) ELSE ''13-may-2013'' END))
         GROUP  BY Start         
END

IF @EndDate >= ''13-may-2013'' 
BEGIN
INSERT INTO #Stats2 SELECT Start, SUM(Guests) AS Guests, SUM(ReservedSeats)  AS ReservedSeats, ISNULL(SUM(NumberOfGuests), 0) AS NumberOfGuests,
				SUM(AmountToChef) AS AmountToChef, SUM(AmountToChef)* 0.1 AS GCcommission, SUM(AmountToChef)*1.1 AS TotalSales 
         FROM   Event E 
                LEFT JOIN (SELECT EventId,Cost,SUM(NumberOfGuests)AS NumberOfGuests,Cost * SUM(Numberofguests) AS AmountToChef 
								FROM   dbo.[Eventattendee] EA 
                                INNER JOIN dbo.[Event] E ON E.Id = EA.Eventid 
								WHERE  Active = 1 AND (Start >= (CASE WHEN @StartDate >= ''13-may-2013'' THEN @StartDate ELSE ''13-may-2013'' END) AND Start <= DATEADD(dd,1,@EndDate))
								GROUP  BY Eventid,Cost)B 
                ON E.Id = B.Eventid 
         WHERE  Active = 1 AND (Start > (CASE WHEN @StartDate >= ''13-may-2013'' THEN @StartDate ELSE ''13-may-2013'' END) AND Start <= DATEADD(dd,1,@EndDate))
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


