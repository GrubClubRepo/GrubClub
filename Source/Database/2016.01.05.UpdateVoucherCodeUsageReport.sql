-- Author:	Swati Agrawal
-- Summary:	added few extra columns to indicate the type of voucher and balance availability

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.01.05.UpdateVoucherCodeUsageReport'

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
SET Query='
SELECT v.code                         AS VoucherCode, 
       v.description                  AS VoucherDescription, 
       u.firstname + '' '' + u.lastname AS GuestName, 
       a.email                        AS Email, 
       bookingdate                    AS BookingDate, 
       e.NAME                         AS EventName, 
       e.start                        AS EventDate, 
       Sum(numberofguests)            AS NumberOfGuests, 
       Sum(guestbaseprice)            AS OriginalPrice, 
       Sum(discount)                  AS Discount, 
       Sum(totalprice)                AS ToatlAmountPaidByGuest, 
       case when v.hostname is not null then v.hostname else s.Name end AS HostName,
       CASE 
		 when v.typeId = 5 then ''Gift Voucher''
         WHEN sv.voucherid IS NOT NULL THEN ''Host'' 
         WHEN v.ownerid = 2 THEN ''Admin'' 
         ELSE ''Host'' 
       END                            AS Owner, 
       case when v.TypeId=1 then ''%'' when v.TypeId=2 then ''£'' else '''' end as DiscountType,
       v.offvalue AS DiscountValue,     
       CASE when v.typeId = 5 then v.AvailableBalance else 0 end as GiftVoucherBalance
FROM   (SELECT ea.userid, 
               ea.eventid, 
               ea.voucherid, 
               bookingdate, 
               Sum(numberofguests) AS numberofguests, 
               Sum(guestbaseprice) AS guestbaseprice, 
               Sum(discount)       AS discount, 
               Sum(ea.totalprice)  AS totalprice 
        FROM   eventattendee ea with(nolock)
        where voucherid is not null
        GROUP  BY ea.userid, 
                  ea.eventid, 
                  ea.voucherid, 
                  bookingdate )x 
       INNER JOIN event e with(nolock)
         ON e.id = x.eventid 
       INNER JOIN [user] u with(nolock)
         ON u.id = x.userid 
       INNER JOIN aspnet_membership a with(nolock)
         ON u.id = a.userid 
       INNER JOIN voucher v with(nolock)
         ON v.id = x.voucherid 
       LEFT JOIN supperclubvoucher sv with(nolock)
              ON sv.voucherid = v.id 
       INNER JOIN SupperClub s with(nolock)
         ON e.SupperClubId = s.id
GROUP  BY bookingdate, 
          v.code, 
          v.description, 
          v.TypeId,
          u.firstname, 
          u.lastname, 
          a.email, 
          e.NAME, 
          e.start, 
          v.ownerid, 
          v.hostname, 
          sv.voucherid,
          s.Name,
          v.OffValue,
          v.AvailableBalance
ORDER  BY v.code, 
          e.NAME  '
WHERE Name= 'Voucher Code Usage' AND ReportType=4

	
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
