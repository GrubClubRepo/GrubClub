-- Author:	Supraja
-- Summary:	updated report with voucher owner

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2015.11.24.UpdateVoucherCodeUsageReport'

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
SET Query=N'SELECT v.code                         AS VoucherCode, 
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
	   
	   case when sv.voucherid is not null then ''Host''
	    	   when v.OwnerId =2 then ''Admin''
	   ELSE ''Host'' End as Owner,
	   v.HostName 
FROM   (SELECT ea.userid, 
               ea.eventid, 
               case when ea.VoucherId is null then y.voucherId else ea.VoucherId end AS VoucherId, 
               bookingdate, 
               Sum(numberofguests) AS numberofguests, 
               Sum(guestbaseprice) AS guestbaseprice, 
               Sum(discount)       AS discount, 
               Sum(ea.totalprice)  AS totalprice 
        FROM   (SELECT DISTINCT ea.id, 
                                t.voucherid 
                FROM   eventattendee ea 
                       JOIN ticket t 
                         ON ea.eventid = t.eventid 
                            AND ea.userid = t.userid 
                            AND ea.seatingid = t.seatingid 
                            AND ea.menuoptionid = t.menuoptionid 
                WHERE  t.voucherid > 0)y 
               JOIN eventattendee ea 
                 ON ea.id = y.id 
        GROUP  BY ea.userid, 
                  ea.eventid, 
                  y.voucherid,
                  ea.VoucherId, 
                  bookingdate)x 
       JOIN event e 
         ON e.id = x.eventid 
       JOIN [user] u 
         ON u.id = x.userid 
       JOIN aspnet_membership a 
         ON u.id = a.userid 
       JOIN voucher v 
         ON v.id = x.voucherid 
		 left join supperclubvoucher sv on sv.voucherid=v.id
	
GROUP  BY bookingdate, 
          v.code, 
          v.description, 
          u.firstname, 
          u.lastname, 
          a.email, 
          e.NAME, 
          e.start,v.OwnerId , v.HostName ,sv.voucherid
ORDER  BY v.code, 
          e.NAME'
WHERE Name= 'Voucher Code Usage'  AND ReportType=4

	
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
