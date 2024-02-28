﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace SupperClub.Code
{
    public class SimplerAES
    {
        // Static keys for simplicity. This is only used to decode a Review auth token back to a specific eventId and userId
        private static byte[] key = { 8, 217, 19, 11, 5, 26, 85, 45, 114, 4, 27, 162, 37, 112, 222, 34, 241, 24, 175, 144, 16, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
        private static byte[] vector = { 5, 64, 191, 111, 23, 3, 113, 10, 231, 121, 221, 112, 79, 32, 18, 156 };
        private ICryptoTransform encryptor, decryptor;
        private UTF8Encoding encoder;

        public SimplerAES()
        {
            RijndaelManaged rm = new RijndaelManaged();
            encryptor = rm.CreateEncryptor(key, vector);
            decryptor = rm.CreateDecryptor(key, vector);
            encoder = new UTF8Encoding();
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public string EncryptToUrl(string unencrypted)
        {
            return HttpUtility.UrlEncode(Encrypt(unencrypted));
        }

        public string DecryptFromUrl(string encrypted)
        {
            return Decrypt(HttpUtility.UrlDecode(encrypted));
        }

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
}