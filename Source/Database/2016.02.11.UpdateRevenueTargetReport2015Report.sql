-- Author:	Supraja
-- Summary:	Updated Cost based on Event Commission

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.02.11.UpdateRevenueTargetReport2015Report'

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
update report
set query='
DECLARE @YearStartDate DATETIME, @StartCurrentMonth DATETIME
SET @YearStartDate = DATEADD(yy, DATEDIFF(yy,0,getdate()), 0) 
SET @StartCurrentMonth = DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0) 


select [month], 
ROUND(eventCost, 2) AS InventoryAvailableForSale,
ROUND(guestbaseprice, 2) AS TotalSalesIncludingVouchers,
ROUND((guestbaseprice - TotalPrice), 2) AS TotalValueOfAllVouchersApplied,
ROUND(TotalPrice, 2) AS ActualSalesPaidThroughSagePay,
ROUND(AverageTicketPrice, 2) as AverageTicketPrice,
guests AS TotalSeatsAvailableIncReserved,
numberofguests AS TotalSeatsSold,
ROUND((case when guests = 0 then 0 else (CAST(numberofguests as float)*100/guests) end), 2) AS FillRateReserved,
(guests - ReservedSeats) AS TotalSeatsAvailableExcReserved,
reservedseats AS TotalReservedSeats,
ROUND(case when(guests - ReservedSeats) = 0 then 0 else (CAST(numberofguests as float)*100)/(guests - ReservedSeats) end, 2) AS FillRate,
ROUND(case when EventCount = 0 then 0 else guestbaseprice/EventCount end, 2) AS RevenuePerEvent,
EventCount AS NumberofEvents,
ROUND(case when ChefCount = 0 then 0 else CAST(EventCount as float)/ChefCount end, 2) AS NumberofEventsPerChef,
ChefCount AS NumberofChefs
from(
select Datename(month, Dateadd(month, [MONTH], 0) - 1) AS [month],EventCount, ChefCount, guests,reservedseats, eventCost, numberofguests, cost, guestbaseprice, totalPrice, case when numberofguests = 0 then 0 else guestbaseprice/numberofguests end AS AverageTicketPrice
 from
(
(select Month(start) AS [month], 
COUNT(eventid) AS EventCount, 
COUNT(distinct SupperClubid) AS ChefCount, 
SUM(guests) AS guests,
SUM(reservedseats) as reservedseats, 
SUM(numberofguests) as numberofguests, 
sum(cost) as cost, 
sum(guestbaseprice) as guestbaseprice, 
sum(totalPrice) as totalPrice,
sum(eventCost) AS eventCost
from (select e.id as eventid, e.supperclubid,e.start,(e.cost+( e.cost*e.commission/100)) AS Cost,      
			CASE 
               WHEN ES.eventid IS NULL THEN Sum(e.guests) 
               ELSE Sum(es.guests) 
             END as guests,                                                         
             CASE 
               WHEN ES.eventid IS NULL THEN Sum(e.reservedseats) 
               ELSE Sum(es.reservedseats) 
             END as reservedseats,
             (CASE 
               WHEN ES.eventid IS NULL THEN Sum(e.guests) 
               ELSE Sum(es.guests) 
             END )*(e.cost+( e.cost*e.commission/100)) AS EventCost,
             numberofguests,
             GuestBasePrice,
             TotalPrice                                                                    
			from Event e 
			inner join SupperClub s on e.Supperclubid = s.id
			inner join (select eventid,SUM(numberofguests) as numberofguests, sum(GuestBasePrice) AS GuestBasePrice, SUM(TotalPrice) AS TotalPrice from eventattendee group by eventid)ea on ea.EventId = e.Id
			left join EventSeating es on e.Id = es.EventId
			where e.Status = 1 and s.Active =1 and e.cost > 0  and (e.start >= @YearStartDate and e.Start < @StartCurrentMonth)
			group by e.id, e.supperclubid,e.start,numberofguests, GuestBasePrice, TotalPrice, e.Cost, e.commission,es.eventid
			)temp
		GROUP  BY month(start))
union all
(select Month(start) AS [month], 
COUNT(eventid) AS EventCount, 
COUNT(distinct SupperClubid) AS ChefCount, 
SUM(guests) AS guests,
SUM(reservedseats) as reservedseats, 
SUM(numberofguests) as numberofguests, 
sum(cost) as cost, 
sum(guestbaseprice) as guestbaseprice, 
sum(totalPrice) as totalPrice,
sum(eventCost) AS eventCost
from (select e.id as eventid, e.supperclubid,e.start, e.cost*1.1 AS Cost,      
			CASE 
               WHEN ES.eventid IS NULL THEN Sum(e.guests) 
               ELSE Sum(es.guests) 
             END as guests,                                                         
             CASE 
               WHEN ES.eventid IS NULL THEN Sum(e.reservedseats) 
               ELSE Sum(es.reservedseats) 
             END as reservedseats,
             (CASE 
               WHEN ES.eventid IS NULL THEN Sum(e.guests) 
               ELSE Sum(es.guests) 
             END )*(e.cost+( e.cost*e.commission/100)) AS EventCost,
             case when ea.EventId is null then 0 else numberofguests end as numberofguests,
             case when ea.EventId is null then 0 else GuestBasePrice end as GuestBasePrice,
             case when ea.EventId is null then 0 else TotalPrice end as TotalPrice                                                                
			from Event e 
			inner join SupperClub s on e.Supperclubid = s.id
			left join (select eventid,SUM(numberofguests) as numberofguests, sum(GuestBasePrice) AS GuestBasePrice, SUM(TotalPrice) AS TotalPrice from eventattendee group by eventid)ea on ea.EventId = e.Id
			left join EventSeating es on e.Id = es.EventId
			where e.Status = 1 and s.Active =1 and e.cost > 0 and (e.start >= @StartCurrentMonth)
			group by e.id, e.supperclubid,e.start,numberofguests, GuestBasePrice, TotalPrice, e.Cost,e.commission, es.eventid,ea.eventid
			)temp
		GROUP  BY month(start))
)a)b
'where id='32' and name='Revenue Target 2015 Report'

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
