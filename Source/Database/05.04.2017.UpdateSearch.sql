-- Author:	Swati
-- Summary:	Updated event description formatting

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='05.04.2017.UpdateSearch'

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

	EXEC sp_executesql N'ALTER PROCEDURE [dbo].[Search] 
  --''4/12/2014'',3,10,2,10,100,'''','''',null,null,'''',null,null                         
  -- EXEC Search 100, 1,'''','''', 0.0,0.0,'''','''','''', null                    
  -- EXEC Search ''01-sep-2015'',100, 10, 2,0,200,'''','''', 0.0,0.0,'''',null,null                                                            
  --@StartDate datetime,                                                                
  --@EndDateOffset int,                                                                     
  @Distance    INT, 
  @Guests      INT, 
  @FoodKeyword NVARCHAR(50), 
  @Diet        NVARCHAR(50), 
  @Latitude    FLOAT = NULL, 
  @Longitude   FLOAT = NULL, 
  @QueryTag    NVARCHAR(50) = NULL, 
  @QueryCity   NVARCHAR(50) = NULL, 
  @QueryArea   NVARCHAR(50) = NULL, 
  @UserId      UNIQUEIDENTIFIER=NULL 
AS 
  BEGIN 
      SET nocount ON; 

      DECLARE @StartPoint GEOGRAPHY; 

      IF ( ( @Latitude IS NOT NULL 
             AND @Latitude <> 0 ) 
           AND ( @Longitude IS NOT NULL 
                 AND @Latitude <> 0 ) ) 
        BEGIN 
            SET @StartPoint = geography::STGeomFromText( 
                              ''Point('' + Cast(@Longitude 
                              AS 
                              VARCHAR(32)) 
                              + '' '' + Cast(@Latitude AS 
                              VARCHAR(32 
                              )) + '')'', 4326) 
        END 

      DECLARE @EndDate DATETIME 
      DECLARE @StartDate DATETIME 

      SET @StartDate = Dateadd(hh, 1, Getdate()) 
      SET @EndDate= Dateadd(dd, 100, @StartDate) 

      DECLARE @FoodKeywordWithoutSpaces NVARCHAR(50) 

      IF ( @FoodKeyword IS NOT NULL 
           AND @FoodKeyword <> '''' ) 
        BEGIN 
            SET @FoodKeywordWithoutSpaces = Replace(@FoodKeyword, '' '', '''') 

            PRINT @FoodKeywordWithoutSpaces 
        END 

      CREATE TABLE #otherdateoptions 
        ( 
           NAME           NVARCHAR(500) COLLATE latin1_general_ci_as, 
           seatsavailable BIT, 
           eventcount     INT 
        ) 

      INSERT INTO #otherdateoptions 
                  (NAME, 
                   seatsavailable, 
                   eventcount) 
      SELECT e.NAME, 
             CASE 
               WHEN ( ( Sum(CASE 
                              WHEN es.eventid IS NULL THEN e.guests 
                              ELSE es.guests 
                            END) ) + ( Sum(CASE 
                                             WHEN es.eventid IS NULL THEN 
                                             e.reservedseats 
                                             ELSE es.reservedseats 
                                           END) ) ) > 
                    Sum(Isnull(ea.numberofguests, 0)) THEN 
               1 
               ELSE 0 
             END         AS SeatsAvailable, 
             Count(e.id) AS EventCount 
      FROM   event e WITH(nolock) 
             INNER JOIN supperclub s WITH(nolock) 
                     ON e.supperclubid = s.id 
             LEFT JOIN (SELECT eventid, 
                               Sum(guests)        AS Guests, 
                               Sum(reservedseats) AS reservedseats 
                        FROM   eventseating WITH(nolock) 
                        GROUP  BY eventid)ES 
                    ON ES.eventid = e.id 
             LEFT JOIN (SELECT eventid, 
                               Sum(numberofguests) AS numberofguests 
                        FROM   eventattendee WITH(nolock) 
                        GROUP  BY eventid) ea 
                    ON ea.eventid = e.id 
      WHERE  e.start > @StartDate 
             AND e.start < @EndDate 
             AND E.[status] = 1 
             AND S.active = 1 
             AND E.private = 0 
      GROUP  BY e.NAME 

	  DECLARE @NewLineChar AS CHAR(2) = CHAR(13) + CHAR(10)
           
      SELECT DISTINCT E.id 
                      AS EventId, 
                      E.NAME 
                      AS EventName, 
                      REPLACE(E.[Description],''|&|'',@NewLineChar) 
					  As EventDescription,
                      E.imagepath 
                      AS EventImage, 
                      E.start 
                      AS EventDateTime, 
                      LEFT(CONVERT(VARCHAR, Isnull(MS.minstart, E.start), 8), 5) 
                      AS EventStart, 
                      LEFT(CONVERT(VARCHAR, Isnull(MS.minend, E.[end]), 8), 5) 
                      AS EventEnd, 
      CONVERT(VARCHAR, 
      Round(CONVERT(DECIMAL(8, 2), E.cost + ( E.cost * ( 
                                              E.commission / 100 ) )), 1)) AS 
                      Cost 
                      , 
      Isnull(ES.guests, E.guests) 
                      AS 
                      TotalSeats, 
      Isnull(ES.reservedseats, E.reservedseats) 
      + Isnull(Sum(EA.numberofguests), 0)                                  AS 
                      GuestsAttending, 
      --CASE WHEN ES.Id IS NULL THEN ISNULL(SUM(ET.NumberOfGuests),0) ELSE ISNULL(SUM(CASE WHEN ET.SeatingId = ES.ID THEN ET.NumberOfGuests ELSE 0 END),0) END As GuestsAttending,                                   
      E.latitude                                                           AS 
                      lat, 
      E.longitude                                                          AS 
                      lng, 
      E.postcode                                                           AS 
                      EventPostCode, 
      E.urlfriendlyname                                                    AS 
                      EventUrlFriendlyName, 
      E.multiseating, 
      S.NAME                                                               AS 
                      GrubClubName, 
      S.id                                                                 AS 
                      SupperclubId, 
      S.urlfriendlyname                                                    AS 
                      GrubClubUrlFriendlyName, 
      E.earlybird, 
      CONVERT(VARCHAR, CONVERT(DECIMAL(8, 2), E.earlybirdprice))           AS 
                      EarlyBirdPrice, 
      --S.Latitude As lat,                                                                   
      --S.Longitude as lng,                                                                  
      --S.Address,                                                    
      --S.Address2,                                                                    
      --S.City,                                                                    
      --S.Country,                                                    
      --S.Name As SupperClubName,                                                                    
      --S.TradingName As SupperClubTradingName,                                   
      --S.Summary,                                                                   
      ED.dietid, 
      EC.cuisineid, 
      ETG.tagid, 
      @Latitude                                                            AS 
                      CurrentLat, 
      @Longitude                                                           AS 
                      CurrentLgt, 
      CASE 
        WHEN @StartPoint IS NULL THEN 25 
        ELSE ( geography::STGeomFromText(''Point('' 
                                         + Cast(E.longitude AS VARCHAR(32)) + 
                                         '' '' 
                                         + Cast(E.latitude AS VARCHAR(32)) + 
               '')'', 4326).STDistance(@StartPoint) ) 
      END                                                                  AS 
                      Distance 
                      , 
      CASE 
        WHEN ( (SELECT Count(*) 
                FROM   event 
                WHERE  supperclubid = S.id) <= 2 ) THEN ''1'' 
        ELSE ''0'' 
      END                                                                  AS 
                      BrandNew 
                      , 
      E.alcohol 
                      AS Byob, 
      Isnull(Rw.reviewcount, 0)                                            AS 
                      ReviewCount, 
      Isnull(Rw.rating, 0)                                                 AS 
                      Rating, 
      CASE 
        WHEN F.userid IS NULL THEN 0 
        ELSE 1 
      END                                                                  AS 
                      WishEvent, 
      ODO.eventcount, 
      ODO.seatsavailable, 
      E.charity 
      FROM   supperclub S WITH(readuncommitted) 
             INNER JOIN [event] E WITH(readuncommitted) 
                     ON S.id = E.supperclubid 
             INNER JOIN #otherdateoptions ODO 
                     ON ODO.NAME COLLATE database_default = 
                        E.NAME COLLATE database_default 
             LEFT JOIN userfavouriteevent F WITH(readuncommitted) 
                    ON E.id = F.eventid 
                       AND F.userid = @UserId 
             LEFT JOIN [user] u 
                    ON u.id = s.userid 
             --LEFT JOIN  REVIEW re on  re.eventid=E.id                       
             LEFT JOIN (SELECT sc.id, 
                               Count(re.id) AS ReviewCount, 
                               CASE 
                                 WHEN Sum(CASE 
                                            WHEN re.rating IS NULL THEN 0 
                                            ELSE 1 
                                          END) = 0 THEN 0 
                                 ELSE ( Sum(Isnull(re.rating, 0)) / Sum( 
                                        CASE 
                                          WHEN re.rating IS 
                                               NULL THEN 0 
                                          ELSE 1 
                                        END) ) 
                               END          AS Rating 
                        FROM   review re 
                               INNER JOIN event e 
                                       ON re.eventid = e.id 
                               INNER JOIN supperclub sc 
                                       ON sc.id = e.supperclubid 
                        GROUP  BY sc.id) Rw 
                    ON S.id = Rw.id 
             INNER JOIN (SELECT DISTINCT eventid, 
                                         A.tagid 
                         FROM  (SELECT DISTINCT eventid, 
                                                NAME, 
                                                ET.tagid 
                                FROM   [eventtag] ET WITH(readuncommitted) 
                                       INNER JOIN [tag] T WITH(readuncommitted) 
                                               ON T.id = ET.tagid 
                                WHERE  @QueryTag = '''' 
                                        OR T.urlfriendlyname = @QueryTag)A) ETG 
                     ON E.id = ETG.eventid 
             LEFT JOIN (SELECT DISTINCT ec.eventid 
                        FROM   eventcity ec 
                               INNER JOIN city c 
                                       ON ec.cityid = c.id 
                        WHERE  @QueryCity = '''' 
                                OR c.urlfriendlyname = @QueryCity)ECT 
                    ON ECT.eventid = E.id 
             LEFT JOIN (SELECT DISTINCT ea.eventid 
                        FROM   eventarea ea 
                               INNER JOIN area a 
                                       ON ea.areaid = a.id 
                               INNER JOIN city c 
                                       ON a.cityid = c.id 
                        WHERE  ( @Queryarea = '''' 
                                  OR a.urlfriendlyname = @QueryArea ) 
                               AND ( @QueryCity = '''' 
                                      OR c.urlfriendlyname = @QueryCity ))EAT 
                    ON EAT.eventid = E.id 
             LEFT JOIN [menu] M WITH(readuncommitted) 
                    ON E.id = M.eventid 
             LEFT JOIN [eventdiet] ED WITH(readuncommitted) 
                    ON E.id = ED.eventid 
             LEFT JOIN [eventcuisine] EC WITH(readuncommitted) 
                    ON E.id = EC.eventid 
             LEFT JOIN (SELECT eventid, 
                               Sum(guests)        AS Guests, 
                               Sum(reservedseats) AS ReservedSeats 
                        FROM   [eventseating] WITH(readuncommitted) 
                        GROUP  BY eventid) ES 
                    ON E.id = ES.eventid 
             LEFT JOIN [eventattendee] EA WITH(readuncommitted) 
                    ON E.id = EA.eventid 
             LEFT JOIN (SELECT E.id           AS EventId, 
                               Min(EST.start) AS MinStart, 
                               Min(EST.[end]) AS MinEnd 
                        FROM   [event] E WITH(readuncommitted) 
                               LEFT JOIN [eventseating] EST WITH(readuncommitted 
                                         ) 
                                      ON E.id = EST.eventid 
                        GROUP  BY E.id) MS 
                    ON MS.eventid = E.id 
      WHERE 
        -- (@Charity is null OR E.Charity = @Charity)                    
        --AND                                                                   
        -- (@Alcohol is null OR E.Alcohol = @Alcohol)                                                                  
        --AND                                                                   
        CONVERT (DATE, E.start) >= CONVERT (DATE, @StartDate) 
        AND CONVERT (DATE, E.start) <= CONVERT (DATE, @EndDate) 
        AND 
        -- ROUND(E.Cost*1.1,2) <= @MaxPrice                                                                  
        --AND                                                                   
        -- ROUND(E.Cost*1.1,2) >= @MinPrice                           
        --AND                                                                   
        ( M.menuitem LIKE ''%'' + @FoodKeyword + ''%'' 
           OR E.NAME LIKE ''%'' + @FoodKeyword + ''%'' 
           OR E.description LIKE ''%'' + @FoodKeyword + ''%'' 
           OR S.NAME LIKE ''%'' + @FoodKeyword + ''%'' 
           OR s.description LIKE ''%'' + @FoodKeyword + ''%'' 
           OR U.firstname LIKE ''%'' + @FoodKeyword + ''%'' 
           OR u.lastname LIKE ''%'' + @FoodKeyword + ''%'' 
           OR u.firstname + '' '' + u.lastname LIKE ''%'' + @FoodKeyword + ''%'' 
           OR Replace(M.menuitem, '' '', '''') LIKE 
              ''%'' + @FoodKeywordWithoutSpaces + 
              ''%'' 
           OR Replace(E.NAME, '' '', '''') LIKE ''%'' + @FoodKeywordWithoutSpaces + 
                                            ''%'' 
           OR Replace(E.description, '' '', '''') LIKE 
              ''%'' + @FoodKeywordWithoutSpaces + ''%'' 
           OR Replace(S.NAME, '' '', '''') LIKE ''%'' + @FoodKeywordWithoutSpaces + 
                                            ''%'' 
           OR Replace(s.description, '' '', '''') LIKE 
              ''%'' + @FoodKeywordWithoutSpaces + ''%'' 
           OR U.firstname LIKE ''%'' + @FoodKeywordWithoutSpaces + ''%'' 
           OR u.lastname LIKE ''%'' + @FoodKeywordWithoutSpaces + ''%'' 
           OR u.firstname + '''' + u.lastname LIKE 
              ''%'' + @FoodKeywordWithoutSpaces + ''%'' 
        ) 
        AND ( @StartPoint IS NULL 
               OR geography::STGeomFromText(''Point('' 
                                            + Cast(E.longitude AS VARCHAR(32)) + 
                                            '' '' 
                                            + Cast(E.latitude AS VARCHAR(32)) + 
                  '')'', 4326).STDistance(@StartPoint) <= ( 
                  @Distance / 0.000621371192 ) 
            ) 
        AND E.[status] = 1 
        AND S.active = ''True'' 
        AND E.private = ''False'' 
        AND Isnull(ECT.eventid, 0) > CASE 
                                       WHEN ( @QueryCity IS NULL 
                                               OR @QueryCity = '''' ) THEN -1 
                                       ELSE 0 
                                     END 
        AND Isnull(EAT.eventid, 0) > CASE 
                                       WHEN ( @QueryArea IS NULL 
                                               OR @QueryArea = '''' ) THEN -1 
                                       ELSE 0 
                                     END 
      GROUP  BY E.id, 
                E.NAME, 
                E.[description], 
                E.imagepath, 
                E.[start], 
                E.[end], 
                E.guests, 
                E.reservedseats, 
                E.cost, 
                E.commission, 
                E.latitude, 
                E.longitude, 
                E.postcode, 
                E.urlfriendlyname, 
                E.multiseating, 
                S.NAME, 
                S.urlfriendlyname, 
                ODO.eventcount, 
                ODO.seatsavailable, 
                --S.Latitude ,                                                                    
                --S.Longitude,           
                --S.Address,                                                                 
                --S.Address2,                                                                    
                --S.City,                                                        
                --S.Country,                                                                    
                --S.Name ,                                                                   
                --S.TradingName,                                                                    
                --S.Summary,                                                                   
                ED.dietid, 
                EC.cuisineid, 
                ETG.tagid, 
                ES.guests, 
                ES.reservedseats, 
                MS.minstart, 
                MS.minend, 
                S.id, 
                E.alcohol, 
                E.charity, 
                Rw.reviewcount, 
                Rw.rating, 
                E.earlybirdprice, 
                E.earlybird, 
                F.userid 
      --Having( E.Guests - E.ReservedSeats - ISNULL(SUM(ET.NumberOfGuests),0) ) >= @Guests                                                                  
      ORDER  BY 
      --GEOGRAPHY::STGeomFromText(''Point('' + CAST(E.Longitude AS VARCHAR(32)) + '' '' + CAST(E.Latitude AS VARCHAR(32)) + '')'',4326).STDistance(@StartPoint),                                                                
      E.start ASC 

      DROP TABLE #otherdateoptions 
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