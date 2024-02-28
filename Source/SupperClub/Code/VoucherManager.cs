using SupperClub.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub;
using SupperClub.Data;

namespace SupperClub.Code
{
    public class VoucherManager
    {

        public Voucher CreateVoucher(string Description,Decimal Price,int Type)
        {           
            Voucher voucher = new Voucher();
            Voucher v1 = new Voucher();

            try
            {
                voucher.UniqueUserRedeemLimit = 0;//model.UsageCap;
                voucher.CreatedDate = DateTime.Now;
                // bool dtValid = true;
                // string sError = "";
                voucher.Description = Description;

                voucher.TypeId = Type;
                voucher.OwnerId = 2;
                voucher.OffValue = Price;//Convert.ToDecimal("10.00");
                voucher.AvailableBalance = Price;
                voucher.IsGlobal = true;
                voucher.Active = true;
                if (Type == 1)
                    voucher.ExpiryDate = DateTime.Today.AddMonths(2);
                else
                    voucher.ExpiryDate = DateTime.Today.AddYears(1);
                voucher.StartDate = DateTime.Now;
                voucher.MinBookingAmount = Convert.ToDecimal("0.00");
                voucher.NumberOfTimesUsed = 0;
                voucher.UsageCap = 0;

                SupperClubRepository _supperClubRepository= new SupperClubRepository();
                
                    int i = 0;
                    string voucherCode = "";
                    do
                    {
                        
                        voucherCode = SupperClub.Web.Helpers.Utils.GetUniqueKey(10);
                        bool isExisting = _supperClubRepository.CheckVoucherCode(voucherCode);
                        if (!isExisting)
                        {

                            voucher.Code = voucherCode;
                            i = 1;
                        }
                    }
                    while (i == 0);

                    v1 = _supperClubRepository.CreateVoucher(voucher);
                    //if (v1 != null && v1.Id > 0)
                    //{
                    //    return "Error";
                    //}
                              
               
            }
            catch (Exception ex)
            {
                v1 = voucher;
                throw;

            }

            return v1;
        }

       
    }
}