using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace SupperClub.Code
{
    /// <summary>
    /// Represents class generating password.
    /// </summary>
    public class PasswordGenerator
    {
        /// <summary>
        /// Generates the password.
        /// </summary>
        /// <returns>New password.</returns>
        public static string GeneratePassword()
        {            
            int minNonAlpha = Membership.Provider.MinRequiredNonAlphanumericCharacters;
            int minLength = Math.Max(Membership.Provider.MinRequiredPasswordLength, 8);
            int digitCount = (minLength - minNonAlpha) / 3;
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ01234567890123456789";
            string allowedDigits = "0123456789";
            string allowedNonAlpha = "#$%&@?!";
            char[] mixed = new char[minLength];

            Random randNum = new Random();
            for (int i = 0; i < minLength - minNonAlpha - digitCount; i++)
                mixed[i] = allowedChars[randNum.Next(0, allowedChars.Length)];

            for (int i = 0; i < digitCount; i++)
                mixed[i + minLength - minNonAlpha - digitCount] = allowedDigits[randNum.Next(0, allowedDigits.Length)];

            for (int i = 0; i < minNonAlpha; i++)
                mixed[i + minLength - minNonAlpha] = allowedNonAlpha[randNum.Next(0, allowedNonAlpha.Length)];

            // mix string
            for (int i = 0; i < 15; i++)
            {
                int first = randNum.Next(0, mixed.Length);
                int second = randNum.Next(0, mixed.Length);
                char old = mixed[second];
                mixed[second] = mixed[first];
                mixed[first] = old;
            }

            return new string(mixed);
        }
    }
}