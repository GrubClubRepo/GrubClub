-- Author:	Supraja
-- Summary:	Updated Cost based on Event Commission

DECLARE @ScriptCode nvarchar(50)
SET @ScriptCode='2016.02.11.UpdateReferralVoucherReport'

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
select  s.FirstName+'' ''+s.lastName as ReferrerName, 
a.username as ReferrerEmail,
 u.name as FriendName , 
 u.FriendEmailId as FriendEmailId,
v.offValue as ''Amount(Percentage)'',v.code as vouchercode,
u.createddate as VoucherCreatedDate,
r.RedeemUserName,r.RedeemEmailId,r.EventName,r.RedeemDate,r.ActualEventPrice,r.AmountPaidByUser,r.Tickets
 from  uservouchertypedetail u
join voucher v on u.VoucherId=v.id
join [user] s on s.id=u.UserId
join [aspnet_users] a on s.id=a.userid
left join (

select t.VoucherId, us.FirstName+'' ''+us.LastName as RedeemUserName,

au.UserName as RedeemEmailId, e.name as EventName , 

b.DateCreated as RedeemDate,sum(cast(Round((e.Cost+(e.cost*e.commission/100)),2) as numeric(36,2))) as ActualEventPrice,
sum(cast(Round(ABS(t.TotalPrice-t.DiscountAmount),2) as numeric(36,2))) as AmountPaidByUser,
count(t.voucherid) as Tickets

from [Ticket] t --on t.VoucherId=v.id
left join [user] us on t.UserId=us.Id
left join [aspnet_users] au on au.UserId=us.id
left  join [Event] e on e.id=t.eventid
left  join TicketBasket b on t.BasketId=b.id

group by us.FirstName,us.LastName,au.UserName,e.name,b.DateCreated,t.voucherid
) r on r.VoucherId=v.id
where u.VType=1
order by v.CreatedDate
'where id='28' and name='ReferralVoucher'

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

