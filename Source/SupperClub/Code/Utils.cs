using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using SupperClub.Domain;
using System.Net;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Text;
using SupperClub.Code;
using System.Web.Mvc;
using System.Drawing;
using log4net;
using System.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web.Security;

namespace SupperClub.Web.Helpers
{
    public class Utils
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(Utils));
        protected static SupperClub.Code.SimplerAES sa = new SupperClub.Code.SimplerAES();

        #region SSL Handling

        private static bool forceSSL = bool.Parse(WebConfigurationManager.AppSettings["ForceSSL"]);
        // Below variable would be used with encryption and decryption of the strings
        private static string passPhrase = "ihtmp68mt5s8q8mq";
        /// <summary>
        /// Forces SSL when required by the booking controller, but allows local and test server to override to non-SSL for dev and testing
        /// </summary>
        public class RequireHttpsInProduction : RequireHttpsAttribute
        {
            public override void OnAuthorization(AuthorizationContext filterContext)
            {
                if (filterContext == null)
                {
                    throw new ArgumentNullException("filterContext");
                }

                if (filterContext.HttpContext != null && (filterContext.HttpContext.Request.IsLocal || !forceSSL))
                {
                    return;
                }
                base.OnAuthorization(filterContext);
            }
        }

        /// <summary>
        /// Implemented on the Base Controller to revert to HTTP when SSL is no longer required
        /// Helps stop cross loading failures for external 3rd party libraries
        /// http://lukesampson.com/post/471548689/entering-and-exiting-https-with-asp-net-mvc
        /// </summary>
        public class ExitHttpsIfNotRequiredAttribute : FilterAttribute, IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationContext filterContext)
            {
                // abort if it's not a secure connection  
                if (!filterContext.HttpContext.Request.IsSecureConnection) return;

                // abort if a [RequireHttps] attribute is applied to controller or action  
                if (filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(RequireHttpsAttribute), true).Length > 0) return;
                if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(RequireHttpsAttribute), true).Length > 0) return;

                // abort if a [RetainHttps] attribute is applied to controller or action  
                if (filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(RetainHttpsAttribute), true).Length > 0) return;
                if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(RetainHttpsAttribute), true).Length > 0) return;

                // abort if it's not a GET request - we don't want to be redirecting on a form post  
                if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)) return;

                // redirect to HTTP  
                string url = "http://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
                filterContext.Result = new RedirectResult(url);
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RetainHttpsAttribute : Attribute
        {
            // Do nothing, continue as you were
        }

        #endregion
        
        #region String Stream Helpers

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static string EncodeJsString(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }        

        #endregion

        #region Image Rescaler

        public static Bitmap ScaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            if (ratio >= 1)
                ratio = 1;

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            try
            {
                Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
                Bitmap bmp = new Bitmap(newImage);
                return bmp;
            }
            catch (Exception ex)
            {
                log.Error("ScaleImage: Error resizing image file. " + " Error Message: " + ex.Message + " " + ex.StackTrace);
                return null;
            }
            
        }

        #endregion

        #region FaceBook Utils

        private static string facebook_graph_host = WebConfigurationManager.AppSettings["Facebook_Graph_Host"];
        private static string facebook_client_id = WebConfigurationManager.AppSettings["Facebook_API_Key"];
        private static string facebook_client_secret = WebConfigurationManager.AppSettings["Facebook_API_Secret"];
        private static string facebook_scheme = WebConfigurationManager.AppSettings["Facebook_Scheme"];
        private static string facebook_path = WebConfigurationManager.AppSettings["Facebook_Path"];
        private static string facebook_redirect_uri = WebConfigurationManager.AppSettings["Facebook_Redirect_Uri"];

        public static string GetAccessTokenFromFb(string code)
        {
            //redirectUri must be the same URI that initiates the initial authentication call. in this case,
            // this controller action

            //Create and perform a request using the URI
            //WebRequest request = WebRequest.Create(string.Format(url, appId, redirectUri, appSecret, code));

            try
            {
                string fbhost = string.Format("{0}://{1}", facebook_scheme, facebook_graph_host);
                string fbhostpath = string.Format("{0}/{1}/{2}", fbhost, "oauth", "access_token");
                string fbhostfullpath = string.Format("{0}?client_id={1}&redirect_uri={2}{3}&client_secret={4}&code={5}", fbhostpath, facebook_client_id, ServerMethods.ServerUrl.Replace("www.", ""), facebook_redirect_uri, facebook_client_secret, code);

                WebUILogging.LogLongMessage("Facebook Access Token Request", "Request: " + System.Environment.NewLine + fbhostfullpath, log, Logger.LogLevel.DEBUG);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fbhostfullpath);

                //Read the response as UTF-8 and parse out the access token.
                //Note that the result has access token and expires parameter.
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader streamReader = new StreamReader(stream, encode);
                Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(streamReader.ReadToEnd());

                //string result = streamReader.ReadToEnd();
                //WebUILogging.LogLongMessage("Facebook Access Token request response:", result, log, Logger.LogLevel.DEBUG);
                // Get the token part
                //result = result.Remove(result.IndexOf("&expires"));
                //string accessToken = result.Replace("access_token=", "");
                string accessToken = (string)jObject["access_token"];
                // Clean up
                streamReader.Close();
                response.Close();

                WebUILogging.LogLongMessage("Got Facebook Access Token", "Access Token: " + accessToken, log, Logger.LogLevel.DEBUG);
                return accessToken;
            }
            catch(Exception ex)
            {
                WebUILogging.LogException("Could not get Facebook Access Token", ex, log);
                return null;
            }
        }

        #endregion

        #region Googlemaps API Helpers

        public static GeoLocation GetAddressCoordinates(string postcode, string city, string address = null, string address2 = null)
        {
            
            using (var client = new WebClient())
            {
                XmlDocument doc = new XmlDocument();
                string addressString = (string.IsNullOrEmpty(address) ? "" : address.Replace(" ", "+") + "+") + (string.IsNullOrEmpty(address2) ? "" : address2.Replace(" ", "+") + "+") + (string.IsNullOrEmpty(city) ? "" : city.Replace(" ", "+"));
                String uri = "http://maps.googleapis.com/maps/api/geocode/xml?address=" + addressString.Replace(" ", "+") + "+" + postcode.Replace(" ", "+") + "&region=uk&sensor=false";
                log.Debug("Google Maps URL:" + uri);

                try
                {
                    doc.Load(uri);
                    XmlNode root = doc.DocumentElement;
                    if (root.SelectSingleNode("/GeocodeResponse/status").InnerText == "OK")
                    {
                        double latitude = Double.Parse(root.SelectSingleNode("/GeocodeResponse/result/geometry/location/lat").InnerText);
                        double longitude = Double.Parse(root.SelectSingleNode("/GeocodeResponse/result/geometry/location/lng").InnerText);
                        if (latitude != 0 && longitude != 0)
                            return new GeoLocation(1, latitude, longitude);
                        else
                        {
                            log.Debug("Google maps API call failed(while reading lat/long). Trying to fetch geo info from postcodes.io.");
                            string url = "http://api.postcodes.io/postcodes/" + postcode;
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            WebResponse response = request.GetResponse();
                            Stream stream = response.GetResponseStream();
                            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                            StreamReader streamReader = new StreamReader(stream, encode);
                            string jsonString = streamReader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonString))
                                {
                                    dynamic data = Json.Decode(jsonString);
                                    if (data.status == 200)
                                    {
                                        latitude = (double)data.result.latitude;
                                        longitude = (double)data.result.longitude;
                                        log.Debug("api.postcodes.io response: Latitude=" + latitude.ToString() + " Longitude=" + longitude.ToString());
                                        return new GeoLocation(1, latitude, longitude);
                                    }
                                    else
                                        return null;
                                }
                                else
                                    return null;
                            
                        }
                    }
                    else 
                    {
                        log.Debug("Google maps API call failed. Trying to fetch geo info from postcodes.io.");
                        string url = "http://api.postcodes.io/postcodes/" + postcode;
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        WebResponse response = request.GetResponse();
                        Stream stream = response.GetResponseStream();
                        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                        StreamReader streamReader = new StreamReader(stream, encode);
                        string jsonString = streamReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            dynamic data = Json.Decode(jsonString);
                            if (data.status == 200)
                            {
                                double latitude = (double)data.result.latitude;
                                double longitude = (double)data.result.longitude;
                                log.Debug("api.postcodes.io response: Latitude=" + latitude.ToString() + " Longitude=" + longitude.ToString());
                                return new GeoLocation(1, latitude, longitude);
                            }
                            else
                                return null;
                        }
                        else
                            return null;
                    }                                     
                 }
                 catch (Exception ex)
                 {
                    log.Error("Error fetching Geo Location Info from Google Geo API" + " Input Address: " + address + " Error Message: " + ex.Message + " " + ex.StackTrace);
                    string url = "http://api.postcodes.io/postcodes/" + postcode;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                    StreamReader streamReader = new StreamReader(stream, encode);
                    string jsonString = streamReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        dynamic data = Json.Decode(jsonString);
                        if (data.status == 200)
                        {
                            double latitude = (double)data.result.latitude;
                            double longitude = (double)data.result.longitude;
                            log.Debug("api.postcodes.io response: Latitude=" + latitude.ToString() + " Longitude=" + longitude.ToString());
                            return new GeoLocation(1, latitude, longitude);
                        }
                        else
                            return null;
                    }                    
                }
                 return null;
            }
        }

        #endregion

        #region String Helpers        
        /// <summary>
        /// Returns a Search Engine Friendly URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns>SEO friendly URL</returns>       
        public static string GetSeoFriendlyUrl(string url)
        {
            string encodedUrl = "";

            if (url != null && url != string.Empty)
            {
                // make the url lowercase
                encodedUrl = (url ?? "").ToLower();

                // replace & with and
                encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

                // remove characters
                encodedUrl = encodedUrl.Replace("'", "");

                // remove invalid characters
                encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "-");

                // remove duplicates
                encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

                // trim leading & trailing characters
                encodedUrl = encodedUrl.Trim('-');
            }

            return encodedUrl;
        }
        /// <summary>
        /// Returns nth index of a word/string within a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns>nth index of a string</returns> 
        public static int findStringNthIndex(string target, string value, int n)
        {
            int defaultIndex = -1;
            Match m = Regex.Match(target, "((" + value + ").*?){" + n + "}");

            if (m.Success)
                defaultIndex = m.Groups[2].Captures[n - 1].Index;

            return defaultIndex;
        }
        public static string HTMLEncodeSpecialChars(string text)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(text))
            {
                foreach (char c in text)
                {
                    if (c > 127) // special chars
                        sb.Append(String.Format("&#{0};", (int)c));
                    else
                        sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion

        #region Random String Generator
        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        #endregion

        #region Encryption & Decryption of Strings
        public static class StringCipher
        {
            // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
            // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
            // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
            private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89gfhs347t89u2");

            // This constant is used to determine the keysize of the encryption algorithm.
            private const int keysize = 256;

            public static string Encrypt(string plainText)
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
                {
                    byte[] keyBytes = password.GetBytes(keysize / 8);
                    using (RijndaelManaged symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.Mode = CipherMode.CBC;
                        using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    byte[] cipherTextBytes = memoryStream.ToArray();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }

            public static string Decrypt(string cipherText)
            {
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
                {
                    byte[] keyBytes = password.GetBytes(keysize / 8);
                    using (RijndaelManaged symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.Mode = CipherMode.CBC;
                        using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                        {
                            using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Cookie Helpers
        //public static class CookieProtectionHelperWrapper
        //{

        //    private static MethodInfo _encode;
        //    private static MethodInfo _decode;

        //    static CookieProtectionHelperWrapper()
        //    {
        //        // obtaining a reference to System.Web assembly
        //        Assembly systemWeb = typeof(HttpContext).Assembly;
        //        if (systemWeb == null)
        //        {
        //            throw new InvalidOperationException(
        //                "Unable to load System.Web.");
        //        }
        //        // obtaining a reference to the internal class CookieProtectionHelper
        //        Type cookieProtectionHelper = systemWeb.GetType(
        //                "System.Web.Security.CookieProtectionHelper");
        //        if (cookieProtectionHelper == null)
        //        {
        //            throw new InvalidOperationException(
        //                "Unable to get the internal class CookieProtectionHelper.");
        //        }
        //        // obtaining references to the methods of CookieProtectionHelper class
        //        _encode = cookieProtectionHelper.GetMethod(
        //                "Encode", BindingFlags.NonPublic | BindingFlags.Static);
        //        _decode = cookieProtectionHelper.GetMethod(
        //                "Decode", BindingFlags.NonPublic | BindingFlags.Static);

        //        if (_encode == null || _decode == null)
        //        {
        //            throw new InvalidOperationException(
        //                "Unable to get the methods to invoke.");
        //        }
        //    }

        //    public static string Encode(CookieProtection cookieProtection,
        //                                byte[] buf, int count)
        //    {
        //        return (string)_encode.Invoke(null,
        //                new object[] { cookieProtection, buf, count });
        //    }

        //    public static byte[] Decode(CookieProtection cookieProtection,
        //                                string data)
        //    {
        //        return (byte[])_decode.Invoke(null,
        //                new object[] { cookieProtection, data });
        //    }

        //}
        //public static class MachineKeyCryptography
        //{
        //    public static string Encode(string text, CookieProtection cookieProtection) {
        //        if (string.IsNullOrEmpty(text) || cookieProtection == CookieProtection.None) {
        //            return text;
        //        }
        //        byte[] buf = Encoding.UTF8.GetBytes(text);
        //        return CookieProtectionHelperWrapper.Encode(cookieProtection, buf, buf.Length); 
        //    }

        //    public static string Decode(string text, CookieProtection cookieProtection) {
        //        if (string.IsNullOrEmpty(text)) {
        //            return text;
        //        }
        //        byte[] buf;
        //        try {
        //            buf = CookieProtectionHelperWrapper.Decode(cookieProtection, text);
        //        }
        //        catch(Exception ex) {
        //            throw ex;
        //        }
        //        return Encoding.UTF8.GetString(buf, 0, buf.Length);
        //    }
        //}
        //public static class HttpSecureCookie
        //{
        //    public static HttpCookie Encode(HttpCookie cookie)
        //    {
        //        return Encode(cookie, CookieProtection.All);
        //    }

        //    public static HttpCookie Encode(HttpCookie cookie,
        //                  CookieProtection cookieProtection)
        //    {
        //        HttpCookie encodedCookie = CloneCookie(cookie);
        //        encodedCookie.Value =
        //          MachineKeyCryptography.Encode(cookie.Value, cookieProtection);
        //        return encodedCookie;
        //    }

        //    public static HttpCookie Decode(HttpCookie cookie)
        //    {
        //        return Decode(cookie, CookieProtection.All);
        //    }

        //    public static HttpCookie Decode(HttpCookie cookie,
        //                  CookieProtection cookieProtection)
        //    {
        //        HttpCookie decodedCookie = CloneCookie(cookie);
        //        decodedCookie.Value =
        //          MachineKeyCryptography.Decode(cookie.Value, cookieProtection);
        //        return decodedCookie;
        //    }

        //    public static HttpCookie CloneCookie(HttpCookie cookie)
        //    {
        //        HttpCookie clonedCookie = new HttpCookie(cookie.Name, cookie.Value);
        //        clonedCookie.Domain = cookie.Domain;
        //        clonedCookie.Expires = cookie.Expires;
        //        clonedCookie.HttpOnly = cookie.HttpOnly;
        //        clonedCookie.Path = cookie.Path;
        //        clonedCookie.Secure = cookie.Secure;

        //        return clonedCookie;
        //    }
        //}
        public class CookieStore
        {
            public static void SetCookie(string key, string value, TimeSpan expires)
            {
                HttpCookie encodedCookie = new HttpCookie(key, sa.Encrypt(value));

                if (HttpContext.Current.Request.Cookies[key] != null)
                {
                    var cookieOld = HttpContext.Current.Request.Cookies[key];
                    cookieOld.Expires = DateTime.Now.Add(expires);
                    cookieOld.Value = encodedCookie.Value;
                    HttpContext.Current.Response.Cookies.Add(cookieOld);
                }
                else
                {
                    encodedCookie.Expires = DateTime.Now.Add(expires);
                    HttpContext.Current.Response.Cookies.Add(encodedCookie);
                }
            }
            public static string GetCookie(string key)
            {
                string value = string.Empty;
                HttpCookie cookie = HttpContext.Current.Request.Cookies[key];

                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    value = sa.Decrypt(cookie.Value);
                }
                return value;
            }
        }
         #endregion
        }
}