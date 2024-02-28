-- Author:	Supraja
-- Summary:	Updated Cost with Commission Value

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.03.17.UpdateSearch'

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

	EXEC sp_executesql N'ALTER PROCEDURE [dbo].[Search] --''4/12/2014'',3,10,2,10,100,'''','''',null,null,'''',null,null                        
                      
-- EXEC Search 100, 1,'''','''', 0.0,0.0,'''','''','''', null                   
                     
-- EXEC Search ''01-sep-2015'',100, 10, 2,0,200,'''','''', 0.0,0.0,'''',null,null                                                            
                      
   --@StartDate datetime,                                                               
                      
   --@EndDateOffset int,                                                                    
                      
   @Distance int,                                                          
                      
   @Guests int,                      
                     
   @FoodKeyword nvarchar(50),                                                          
                      
   @Diet nvarchar(50),                                                         
                      
   @Latitude float = null,                                                                  
                      
   @Longitude float = null,                                                        
                      
   @QueryTag nvarchar(50) = null,               
                 
   @QueryCity nvarchar(50) = null,                
                 
   @QueryArea nvarchar(50) = null,          
                         
   @UserId uniqueidentifier=null                                                            
                      
  AS                                                                  
                      
  BEGIN                                                                  
                      
                                                                     
                      
   SET NOCOUNT ON;                                                                  
                      
                                                                     
                      
   DECLARE @StartPoint geography;                     
   IF ((@Latitude IS NOT NULL AND @Latitude <> 0) AND (@Longitude IS NOT NULL AND @Latitude <> 0))                                                          
   BEGIN              
    SET @StartPoint = GEOGRAPHY::STGeomFromText(''Point('' + CAST(@Longitude AS VARCHAR(32)) + '' '' + CAST(@Latitude AS VARCHAR(32)) + '')'',4326)             
   END                                                         
                                                
DECLARE @EndDate DATETIME        
DECLARE @StartDate DATETIME          
 SET @StartDate = Dateadd(hh,1,GETDATE())            
set @EndDate= Dateadd(dd,100,@StartDate)                
              
 DECLARE @FoodKeywordWithoutSpaces nvarchar(50)      
 IF (@FoodKeyword IS NOT NULL AND @FoodKeyword <> '''')      
 BEGIN      
 SET @FoodKeywordWithoutSpaces = REPLACE(@FoodKeyword, '' '','''')      
 print @FoodKeywordWithoutSpaces      
 END             
                      
             
CREATE TABLE #OtherDateOptions(Name nvarchar(500) COLLATE Latin1_General_CI_AS, SeatsAvailable BIT, EventCount INT)                                               
 INSERT INTO #OtherDateOptions(Name,SeatsAvailable,EventCount)                               
 select e.name,             
 case when ((SUM(case when es.EventId is null then e.Guests else es.Guests end)) +              
 (SUM(case when es.EventId is null then e.ReservedSeats else es.ReservedSeats end))) > SUM(isnull(ea.NumberOfGuests, 0)) then 1 else 0 end as SeatsAvailable,            
 COUNT(e.Id) AS EventCount                                                        
          from Event e with(nolock)             
          inner join supperclub s with(nolock) on e.SupperClubId=s.Id            
          left join (select eventid, sum(guests) as Guests, SUM(reservedseats) as reservedseats from EventSeating with(nolock) group by eventid)ES  on ES.EventId = e.Id            
          left join (select eventid, sum(numberofguests) as numberofguests from EventAttendee with(nolock) group by EventId) ea  on ea.EventId=e.Id            
          where e.Start > @StartDate and e.Start <@EndDate and  E.[Status] = 1 AND S.Active = 1 AND E.Private = 0               
          group by e.name            
                      
   Select Distinct                
                      
 E.ID As EventId,                                                                  
                      
    E.Name As EventName,                                               
                      
    E.[Description] As EventDescription,                                                                  
                      
    E.ImagePath As EventImage,                             
                      
                                                                      
                      
    E.Start As EventDateTime,                      
                        
    left(convert(varchar, ISNULL(MS.MinStart, E.Start), 8), 5) As EventStart,                                              
                      
    left(convert(varchar, ISNULL(MS.MinEnd, E.[End]), 8), 5) As EventEnd,                       
                      
                                                                      
                      
    convert(varchar,convert(decimal(8,2),E.Cost+(E.Cost*(E.Commission/100)))) as Cost,                                                                  
                      
    ISNULL(ES.Guests, E.Guests) As TotalSeats,              
    ISNULL(ES.ReservedSeats, E.ReservedSeats) + ISNULL(SUM(EA.NumberOfGuests),0) As GuestsAttending,                                                  
                      
    --CASE WHEN ES.Id IS NULL THEN ISNULL(SUM(ET.NumberOfGuests),0) ELSE ISNULL(SUM(CASE WHEN ET.SeatingId = ES.ID THEN ET.NumberOfGuests ELSE 0 END),0) END As GuestsAttending,                                   
                      
                                                                      
                      
    E.Latitude As lat,                                              
                      
    E.Longitude as lng,                             
                      
    E.PostCode AS EventPostCode,                                      
                      
    E.UrlFriendlyName AS EventUrlFriendlyName,                                                               
                      
    E.MultiSeating,               
	
	              
                      
    S.Name AS GrubClubName,                      
                      
 S.Id as SupperclubId,                      
                      
    S.UrlFriendlyName AS GrubClubUrlFriendlyName,                             
                       
 E.EarlyBird,                      
                       
 convert(varchar,convert(decimal(8,2),E.EarlyBirdPrice)) as EarlyBirdPrice,                                                        
                      
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
                      
         ETG.TagId,                                                             
                      
    @Latitude As CurrentLat,                                                             
                      
    @Longitude As CurrentLgt,                                                                  
                      
    CASE WHEN @StartPoint IS NULL THEN 25 ELSE                                                        
                      
    (GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint)) END As Distance                      
                       
                       
 ,case when ((select count(*) from event where supperclubid=S.Id)<=2) then ''1'' else ''0'' end as BrandNew                      
                      
 ,E.Alcohol as Byob                      
                      
 ,isnull(Rw.ReviewCount,0) AS ReviewCount                     
                      
 ,isnull(Rw.Rating, 0) AS Rating                 
                      
 ,case when F.UserId is  null then 0 else 1 end as WishEvent              
             
 ,ODO.EventCount            
 ,ODO.SeatsAvailable                    
 , E.Charity                      
   From SupperClub S WITH(READUNCOMMITTED)                                                               
                      
    INNER JOIN [Event] E WITH(READUNCOMMITTED) on S.id = E.SupperClubId              
                
    INNER JOIN #OtherDateOptions ODO ON ODO.Name COLLATE DATABASE_DEFAULT = E.Name COLLATE DATABASE_DEFAULT                 
                      
 LEFT JOIN  UserFavouriteEvent F  WITH(READUNCOMMITTED) on  E.id= F.EventId and F.UserId=@UserId                      
                       
 LEFT JOIN  [User] u on  u.id=s.UserId                         
                       
 --LEFT JOIN  REVIEW re on  re.eventid=E.id                      
                       
 LEFT JOIN ( select sc.id , count(re.Id) as ReviewCount,                   
    case when sum(case when re.Rating is null then 0 else 1 end) = 0 then 0 else                  
    (sum(ISNULL(re.Rating,0))/sum(case when re.Rating is null then 0 else 1 end)) end as Rating               
     from Review re                      
    INNER JOIN event e on re.EventId=e.id                      
    INNER JOIN supperclub sc on sc.id=e.SupperClubId                      
    group by sc.id  ) Rw    ON S.Id= Rw.id                      
                      
    INNER JOIN (SELECT Distinct EventId ,A.TagId                                        
                      
     FROM(SELECT Distinct EventId, Name , ET.TagId                                      
                          
         FROM [EventTag] ET WITH(READUNCOMMITTED)                          
                            
         INNER JOIN [Tag] T WITH(READUNCOMMITTED)                                         
                            
         ON T.Id = ET.TagId                                         
                            
         WHERE @QueryTag = ''''                                         
                            
         OR T.UrlFriendlyName = @QueryTag)A                                   
                            
        ) ETG on E.Id = ETG.EventId                                                                  
                    
    LEFT JOIN (select distinct ec.eventId from eventcity ec inner join city c on ec.CityId = c.Id where @QueryCity = '''' OR c.UrlFriendlyName = @QueryCity)ECT on ECT.EventId = E.Id              
                        
    LEFT JOIN (select distinct ea.eventId from eventarea ea inner join area a on ea.AreaId = a.Id inner join city c on a.CityId = c.Id where (@Queryarea = '''' OR a.UrlFriendlyName = @QueryArea) and (@QueryCity = '''' OR c.UrlFriendlyName = @QueryCity))EAT on

    
            
 EAT.EventId = E.Id              
                      
    LEFT JOIN [Menu] M WITH(READUNCOMMITTED) on E.id = M.EventId                                                                
                      
    LEFT JOIN [EventDiet] ED WITH(READUNCOMMITTED) on E.Id = ED.EventId                                                                  
                      
    LEFT JOIN [EventCuisine] EC WITH(READUNCOMMITTED) on E.Id = EC.EventId     
                      
    LEFT JOIN (SELECT EventId, Sum(Guests) AS Guests, SUM(ReservedSeats) AS ReservedSeats from [EventSeating] WITH(READUNCOMMITTED) group by EventId) ES  on E.Id = ES.EventId   
                      
    LEFT JOIN [EventAttendee] EA WITH(READUNCOMMITTED) on E.Id = EA.EventId                                     
                      
    LEFT JOIN (SELECT E.Id AS EventId, MIN(EST.Start) AS MinStart, MIN(EST.[End]) AS MinEnd             
                      
    FROM [Event] E  WITH(READUNCOMMITTED)                                                       
                         
    LEFT JOIN [EventSeating] EST WITH(READUNCOMMITTED)                                         
                         
    on E.Id = EST.EventId GROUP BY E.ID) MS                                         
                      
 ON MS.EventId = E.Id                                                                    
                      
   Where                                                                  
                      
    -- (@Charity is null OR E.Charity = @Charity)                   
                      
    --AND                                                                  
                      
    -- (@Alcohol is null OR E.Alcohol = @Alcohol)                                                                  
                      
    --AND                                                                  
                      
     CONVERT (DATE,E.Start) >= CONVERT (DATE,@StartDate)                            
                      
    AND                                                              
                      
     CONVERT (DATE,E.Start) <= CONVERT (DATE,@EndDate)                                
                      
    AND                                                                  
                      
    -- ROUND(E.Cost*1.1,2) <= @MaxPrice                                                                  
                      
    --AND                                                                  
                      
    -- ROUND(E.Cost*1.1,2) >= @MinPrice                          
                      
    --AND                                                                  
                      
     (M.MenuItem like ''%''+@FoodKeyword +''%'' OR E.Name like ''%''+@FoodKeyword+''%'' OR E.Description like ''%''+@FoodKeyword+''%''                      
                        
  OR  S.Name like ''%''+ @FoodKeyword +''%'' OR s.Description like ''%''+ @FoodKeyword +''%'' OR U.FirstName like ''%''+ @FoodKeyword +''%''                      
                        
  OR u.LastName like ''%''+ @FoodKeyword +''%'' OR u.FirstName+'' ''+u.LastName like ''%''+ @FoodKeyword +''%''       
  OR REPLACE(M.MenuItem, '' '','''') like ''%''+@FoodKeywordWithoutSpaces +''%''       
  OR REPLACE(E.Name,'' '','''') like ''%''+@FoodKeywordWithoutSpaces+''%''       
  OR replace(E.Description,'' '','''') like ''%''+@FoodKeywordWithoutSpaces+''%''        
  OR  replace(S.Name,'' '','''') like ''%''+ @FoodKeywordWithoutSpaces +''%''       
  OR replace(s.Description,'' '','''') like ''%''+ @FoodKeywordWithoutSpaces +''%''       
  OR U.FirstName like ''%''+ @FoodKeywordWithoutSpaces +''%''        
  OR u.LastName like ''%''+ @FoodKeywordWithoutSpaces +''%''       
  OR u.FirstName+''''+u.LastName like ''%''+ @FoodKeywordWithoutSpaces +''%''  )       
  AND                                                            
                      
     (@StartPoint IS NULL OR GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint) <= (@Distance / 0.000621371192))                     
    
      
         
         
             
             
              
                  
                      
    AND                                                                  
                      
     E.[Status] = 1                                                                 
                      
    AND                                                                  
                      
     S.Active = ''True''                 
                      
AND                                                                  
                      
     E.Private = ''False''                                                         
                      
                       
    AND                                                                  
                      
     ISNULL(ECT.EventId, 0) >  case when (@QueryCity is null or @QueryCity ='''') then -1 ELSE 0 end               
                   
     AND                                                                  
                      
     ISNULL(EAT.EventId, 0) >  case when (@QueryArea is null or @QueryArea ='''') then -1 ELSE 0 end                                 
                      
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
                      
    E.Commission,                                                               
                      
    E.Latitude,                            
                    
    E.Longitude,                                
                      
    E.PostCode,                                                   
                      
    E.UrlFriendlyName,                                               
                      
    E.MultiSeating,   
	                    
                      
    S.Name,                         
                      
    S.UrlFriendlyName,                
                
    ODO.EventCount,            
                
    ODO.SeatsAvailable,                                                          
                      
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
                      
    EC.CuisineId,                                                                  
    
	ETG.TagId,                                                             

    ES.Guests,                                                  
                      
    ES.ReservedSeats,                                                          
                      
    MS.MinStart,                      
                      
    MS.MinEnd  ,                                                           
 S.Id,                      
 E.Alcohol,            
 E.Charity                      
                      
 ,Rw.ReviewCount                      
 ,Rw.Rating                      
 ,E.EarlyBirdPrice                      
 ,E.EarlyBird                      
 ,F.UserId                      
   --Having( E.Guests - E.ReservedSeats - ISNULL(SUM(ET.NumberOfGuests),0) ) >= @Guests                                                                  
                      
   Order By                                                         
                      
   --GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint),                                                                
                      
   E.Start ASC                                                                  
               
   DROP TABLE #OtherDateOptions                   
  END '

	

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