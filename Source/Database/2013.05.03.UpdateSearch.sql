-- Author:	Swati Agrawal
-- Summary:	Added UrlFriendlyName column to select statement list for SEO url

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2013.05.03.UpdateSearch'

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

	EXEC sp_executesql N'   
		 ALTER PROCEDURE [dbo].[Search] -- EXEC Search ''03-may-2013'',10, 51.5073346, -0.1276831,2,0,200,'''',''''    
		 @StartDate datetime,    
		 @Distance int,    
		 @Latitude float,    
		 @Longitude float,    
		 @Guests int,    
		 @MinPrice decimal,    
		 @MaxPrice decimal,    
		 @FoodKeyword nvarchar(50),    
		 @Diet nvarchar(50),    
		 @Charity bit = null,    
		 @Alcohol bit = null    
		    
		AS    
		BEGIN    
		     
		 SET NOCOUNT ON;    
		     
			DECLARE @StartPoint geography;    
		 SET @StartPoint = GEOGRAPHY::STGeomFromText(''Point('' + CAST(@Longitude AS VARCHAR(32)) + '' '' + CAST(@Latitude AS VARCHAR(32)) + '')'',4326)    
		 PRINT @StartPoint.ToString()    
		     
		 Select Distinct    
		  E.ID As EventId,    
		  E.Name As EventName,     
		  E.[Description] As EventDescription,    
		  E.ImagePath As EventImage,     
		      
		  convert(varchar, E.Start, 103) As EventDate,    
		  left(convert(varchar, E.Start, 8), 5) As EventStart,    
		  left(convert(varchar, E.[End], 8), 5) As EventEnd,    
		      
		  convert(varchar,convert(decimal(8,2),E.Cost)) as Cost,    
		  E.Guests As TotalSeats,    
		  E.ReservedSeats + ISNULL(SUM(ET.NumberOfGuests),0) As GuestsAttending,    
		      
		  E.Latitude As lat,     
		  E.Longitude as lng,   
		  E.UrlFriendlyName AS EventUrlFriendlyName, 
		      
		  --S.Latitude As lat,     
		  --S.Longitude as lng,    
		      
		  --S.Address,     
		  --S.Address2,     
		  --S.City,     
		  --S.Country,     
		  --S.Name As SupperClubName,     
		  --S.TradingName As SupperClubTradingName,     
		  --S.Summary,    
		    
		  ED.DietId,    
		  EC.CuisineId,    
		      
		  @Latitude As CurrentLat,    
		  @Longitude As CurrentLgt,    
		  GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint) As Distance    
		 From SupperClub S    
		  inner join [Event] E on S.id = E.SupperClubId    
		  left join [EventAttendee] ET on E.Id = ET.EventId     
		  left join [Menu] M on E.id = M.EventId    
		  left join [EventDiet] ED on E.Id = ED.EventId    
		  left join [EventCuisine] EC on E.Id = EC.EventId    
		 Where    
		   (@Charity is null OR E.Charity = @Charity)    
		  AND    
		   (@Alcohol is null OR E.Alcohol = @Alcohol)    
		  AND    
		   E.Start >= @StartDate    
		  AND    
		   E.Cost <= @MaxPrice    
		  AND    
		   E.Cost >= @MinPrice    
		  AND    
		   (M.MenuItem like ''%''+@FoodKeyword +''%'' OR E.Name like ''%''+@FoodKeyword+''%'' OR E.Description like ''%''+@FoodKeyword+''%'')    
		  AND    
		   GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint) <= (@Distance / 0.000621371192)    
		  AND    
		   E.Active = ''True''    
		  AND    
		   S.Active = ''True''    
		  AND    
		   E.Private = ''False''    
		 Group By     
		  E.ID ,    
		  E.Name ,     
		  E.[Description] ,    
		  E.ImagePath ,    
		      
		  E.[Start] ,    
		  E.[End] ,    
		      
		  E.Guests,    
		  E.ReservedSeats,    
		  E.Cost,    
		      
		  E.Latitude,    
		  E.Longitude,    
		  E.UrlFriendlyName,
		      
		  --S.Latitude ,     
		  --S.Longitude,     
		  --S.Address,     
		  --S.Address2,     
		  --S.City,     
		  --S.Country,     
		  --S.Name ,     
		  --S.TradingName,     
		  --S.Summary,    
		      
		  ED.DietId,    
		  EC.CuisineId    
		    
		 --Having( E.Guests - E.ReservedSeats - ISNULL(SUM(ET.NumberOfGuests),0) ) >= @Guests    
		 Order By GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint),    
		 convert(varchar, E.Start, 103) ASC    
		END'

		
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
