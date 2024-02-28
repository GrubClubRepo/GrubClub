using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using SupperClub.WebUI;

namespace SupperClub.Models
{

    public class ChangePasswordModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)] // This must be changed to match the web.config AspNetSqlMembershipProvider passwordLength
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string redirectUrl { get; set; }
    }

    public class RegisterModel
    {
        public string UserName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        //[Required]
        //[Display(Name = "Address")]
        //public string Address { get; set; }

        //[Required]
        //[Display(Name = "Country")]
        //public string Country { get; set; }


        //[Required]
        //[Display(Name = "Post Code")]
        //public string PostCode { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Confirm Email address")]
        [System.ComponentModel.DataAnnotations.Compare("Email", ErrorMessage = "The email address and confirmation email address do not match.")]
        public string ConfirmEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        //[Required]
        //[DataType(DataType.DateTime)]
        //[Display(Name = "Date Of Birth")]
        //public string DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        public string Role { get; set; }
    }

    public class UserSettingsModel
    {
        [Display(Name = "Send me Supper Club newsletters")]
        public bool SendNewsLetters { get; set; }
    }

    public class DeviceInfoModel
    {
        public bool IsMobileDevice { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Screen { get; set; }
        public string Platform { get; set; }
        public string Browser { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
    }
    public class LoginRegisterModel
    {
        public LogOnModel LogOnModel { get; set; }
        public RegisterModel RegisterModel { get; set; }
        public ForgotPasswordModel ForgotPasswordModel { get; set; }
        public string RedirectURL { get; set; }
       
    }

    public class UserDetailModel
    {
        public ChangePasswordModel changePasswordModel
        { get; set; }

        public SupperClub.Domain.User user
        {
            get;
            set;
        }
    }

    public class HostDetailModel
    {
        public SupperClub.Domain.User user
        {
            get;
            set;
        }

        public SupperClub.Domain.SupperClub supperclub
        {
            get;
            set;
        }
    }
}
