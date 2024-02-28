using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Drawing;
using System.IO;
using log4net;

namespace SupperClub.Code
{
    public class ImageScaler
    {
        private static string tempFilesLocation = HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]);
        private static int maxWidth = Convert.ToInt32(WebConfigurationManager.AppSettings["MaxImageWidth"]);
        private static int maxHeight = Convert.ToInt32(WebConfigurationManager.AppSettings["MaxImageHeight"]);
        protected static readonly ILog log = LogManager.GetLogger(typeof(ImageScaler));

        /// <summary>
        /// Takes an image and resizes it for web, returns null if unsuccessful
        /// </summary>
        /// <param name="file">HttpPostedFileBase from the page</param>
        /// <returns>the newFileName if successful, or null if the file couldn't be converted</returns>
        public static string ConvertImage(HttpPostedFileBase file)
        {
            Bitmap original = null;
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            
            string newFileName = Guid.NewGuid().ToString() + ".png";
            string tempFilePath = tempFilesLocation + newFileName;

            try // converting the file
            {
                original = Bitmap.FromStream(file.InputStream) as Bitmap;
                var resizedImage = SupperClub.Web.Helpers.Utils.ScaleImage(original, maxWidth, maxHeight);
                if (resizedImage != null)
                {
                    resizedImage.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    return newFileName;
                }
                else
                {
                    log.Error("Error resizing image file.");
                    return null;
                }
            }
            catch(Exception ex)
            {
                // if the file fails to convert
                log.Error("Error converting image file. file:" + tempFilePath + " Error Message: " + ex.Message + " " + ex.StackTrace + ex.InnerException);
                return null;
            }
        }
    }
}