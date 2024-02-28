using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Models
{
    public class ImagesModel
    {
        public ImagesModel()
        {
            Images = new List<string>();
        }

        public List<string> Images { get; set; }
    }

    public class UploadImageModel
    {
        [Display(Name = "Internet URL")]
        public string Url { get; set; }

        public bool IsUrl { get; set; }

        [Display(Name = "Local file")]
        [SupperClub.Extensions.AnnotationExtensions.File(AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".bmp" }, MaxContentLength = 1024 * 1024 * 10, ErrorMessage = "Invalid File")]
        public HttpPostedFileBase File { get; set; }

        public bool IsFile { get; set; }

        [Range(0, int.MaxValue)]
        public int X { get; set; }

        [Range(0, int.MaxValue)]
        public int Y { get; set; }

        [Range(1, int.MaxValue)]
        public int Width { get; set; }

        [Range(1, int.MaxValue)]
        public int Height { get; set; }

        public string SavedFileName { get; set; }
    }

    
}