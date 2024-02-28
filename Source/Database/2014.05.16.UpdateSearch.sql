-- Author:	Swati Agrawal
-- Summary:	Added logic to include Grub Club Profile Page URL

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2014.05.16.UpdateSearch'

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
ALTER PROCEDURE [dbo].[Search]                                     
-- EXEC Search ''11-mar-2014'',2, 10, 2,0,200,'''','''', 51.5073346, -0.1276831,'''',null,null                                    
-- EXEC Search ''03-jan-2014'',100, 10, 2,0,200,''good'','''', 0.0,0.0,'''',null,null                                      
   @StartDate datetime,                                         
   @EndDateOffset int,                                              
   @Distance int,                                    
   @Guests int,                                            
   @MinPrice decimal,                                            
   @MaxPrice decimal,                                            
   @FoodKeyword nvarchar(50),                                    
   @Diet nvarchar(50),                                   
   @Latitude float = null,                                            
   @Longitude float = null,                                  
   @QueryTag nvarchar(50) = null,                                   
   @Charity bit = null,                                            
   @Alcohol bit = null                                     
  AS                                            
  BEGIN                                            
                                               
   SET NOCOUNT ON;                                            
                                               
   DECLARE @StartPoint geography;                                    
   IF ((@Latitude IS NOT NULL AND @Latitude <> 0) AND (@Longitude IS NOT NULL AND @Latitude <> 0))                                    
   BEGIN                                         
    SET @StartPoint = GEOGRAPHY::STGeomFromText(''Point('' + CAST(@Longitude AS VARCHAR(32)) + '' '' + CAST(@Latitude AS VARCHAR(32)) + '')'',4326)                                            
    PRINT @StartPoint.ToString()                                  
   END                                   
                         
   IF @StartDate <= GETDATE()                                        
 SET @StartDate = Dateadd(hh,1,GETDATE())                                       
                       
                                            
   DECLARE @EndDate DATETIME                                        
   IF @EndDateOffset = 100                                        
   SET @EndDate = Dateadd(yy,100,@StartDate)                                        
   ELSE                                        
   SET @EndDate = Dateadd(dd,@EndDateOffset,@StartDate)                                        
                                            
   Select Distinct                                            
    E.ID As EventId,                                            
    E.Name As EventName,                                             
    E.[Description] As EventDescription,                                            
    E.ImagePath As EventImage,                                            
                                                
    convert(varchar, E.Start, 103) As EventDate,                                            
    left(convert(varchar, ISNULL(MS.MinStart, E.Start), 8), 5) As EventStart,                                            
    left(convert(varchar, ISNULL(MS.MinEnd, E.[End]), 8), 5) As EventEnd,                                            
                                                
    convert(varchar,convert(decimal(8,2),E.Cost)) as Cost,                                            
    E.Guests As TotalSeats,                                            
    ISNULL(ES.ReservedSeats, E.ReservedSeats) + ISNULL(SUM(EA.NumberOfGuests),0) As GuestsAttending,                                            
    --CASE WHEN ES.Id IS NULL THEN ISNULL(SUM(ET.NumberOfGuests),0) ELSE ISNULL(SUM(CASE WHEN ET.SeatingId = ES.ID THEN ET.NumberOfGuests ELSE 0 END),0) END As GuestsAttending,                                            
                                                
    E.Latitude As lat,                        
    E.Longitude as lng,       
    E.PostCode AS EventPostCode,                
    E.UrlFriendlyName AS EventUrlFriendlyName,                                         
    E.MultiSeating,           
    S.Name AS GrubClubName,
    S.UrlFriendlyName AS GrubClubUrlFriendlyName,                                            
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
    CASE WHEN @StartPoint IS NULL THEN 25 ELSE                                  
    (GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint)) END As Distance                                            
   From SupperClub S WITH(READUNCOMMITTED)                                         
    INNER JOIN [Event] E WITH(READUNCOMMITTED) on S.id = E.SupperClubId                     
    INNER JOIN (SELECT Distinct EventId                   
    FROM(SELECT Distinct EventId, Name                  
   FROM [EventTag] ET WITH(READUNCOMMITTED)                                        
   INNER JOIN [Tag] T WITH(READUNCOMMITTED)                   
   ON T.Id = ET.TagId                   
   WHERE @QueryTag = ''''                   
   OR T.UrlFriendlyName = @QueryTag)A                  
  --WHERE @FoodKeyword = '''' OR Name LIKE ''%''+@FoodKeyword+''%''              
  ) ETG                    
    on E.Id = ETG.EventId                                            
    LEFT JOIN [Menu] M WITH(READUNCOMMITTED) on E.id = M.EventId                                            
    LEFT JOIN [EventDiet] ED WITH(READUNCOMMITTED) on E.Id = ED.EventId                                            
    LEFT JOIN [EventCuisine] EC WITH(READUNCOMMITTED) on E.Id = EC.EventId                                         
    LEFT JOIN [EventSeating] ES WITH(READUNCOMMITTED) on E.Id = ES.EventId AND ES.IsDefault = 1                                         
    LEFT JOIN [EventAttendee] EA WITH(READUNCOMMITTED) on E.Id = EA.EventId AND EA.SeatingId = ISNULL(ES.ID,0)               
    LEFT JOIN (SELECT E.Id AS EventId, MIN(EST.Start) AS MinStart, MIN(EST.[End]) AS MinEnd                   
    FROM [Event] E  WITH(READUNCOMMITTED)                                 
    LEFT JOIN [EventSeating] EST WITH(READUNCOMMITTED)                   
    on E.Id = EST.EventId GROUP BY E.ID) MS                   
 ON MS.EventId = E.Id                                              
   Where                                            
     (@Charity is null OR E.Charity = @Charity)                                            
    AND                                            
     (@Alcohol is null OR E.Alcohol = @Alcohol)                                            
    AND                                            
     E.Start > @StartDate                                        
    AND                                        
     E.Start <= @EndDate                                         
    AND                                            
     E.Cost <= @MaxPrice                                            
    AND                                            
     E.Cost >= @MinPrice    
    AND                                            
     (M.MenuItem like ''%''+@FoodKeyword +''%'' OR E.Name like ''%''+@FoodKeyword+''%'' OR E.Description like ''%''+@FoodKeyword+''%'' )                
     AND                                            
     (@StartPoint IS NULL OR GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint) <= (@Distance / 0.000621371192))                                           
    AND                                            
     E.[Status] = 1                                           
    AND                                            
     S.Active = ''True''                                            
    AND                                            
     E.Private = ''False''                                   
    --AND                                            
    -- (@QueryTag = '''' OR T.UrlFriendlyName = @QueryTag)                 
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
    E.PostCode,                                        
    E.UrlFriendlyName,                         
    E.MultiSeating,             
    S.Name,   
    S.UrlFriendlyName,                                      
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
    EA.SeatingId,                                        
    ES.Id,                                    
    ES.ReservedSeats,                                    
    MS.MinStart,                                    
    MS.MinEnd                                       
   --Having( E.Guests - E.ReservedSeats - ISNULL(SUM(ET.NumberOfGuests),0) ) >= @Guests                                            
   Order By                                   
   --GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint),                                            
   convert(varchar, E.Start, 103) ASC                                            
  END  '

		
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
