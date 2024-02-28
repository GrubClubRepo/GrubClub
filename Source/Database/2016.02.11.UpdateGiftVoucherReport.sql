-- Author:	Supraja
-- Summary:	Updated Cost based on Event Commission

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.02.11.UpdateGiftVoucherReport'

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
select  s.FirstName+'' ''+s.lastName as RegisteredName,a.UserName  as RegisteredEmailId, u.name as BuyersName , u.FriendEmailId as BuyersEmailId,
v.offValue as ''amount(Pounds)'',v.code as vouchercode,u.createddate as vouchercreatedate, --t.UserId as RedeemUserid, 
us.FirstName+'' ''+us.LastName as RedeemUserName,au.UserName as RedeemEmailId,
e.name as eventname , b.DateCreated as RedeemDate,cast(Round((e.cost+(e.Cost*e.commission/100)),2) as ActualEventPrice,cast(Round(ABS(t.TotalPrice-t.DiscountAmount),2) as numeric(36,2)) as AmountPaidByUser
 from  uservouchertypedetail u
join voucher v on u.VoucherId=v.id
join [user] s on s.id=u.UserId
left join [aspnet_users] a on a.UserId=s.id
left join [Ticket] t on t.VoucherId=v.id
left join [user] us on t.UserId=us.Id
left join [aspnet_users] au on au.UserId=us.id
left  join [Event] e on e.id=t.eventid
left  join TicketBasket b on t.BasketId=b.id
where u.VType=2
order by v.CreatedDate
'where id='27' and name='GiftVoucher'

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