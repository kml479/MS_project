﻿namespace CourseSales.Web.Models.Catalogs
{
    public class CourseCreateInput
    {
        [Display(Name = "Ad")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        public string ImagePath { get; set; }

        public string UserId { get; set; }

        public FeatureViewModel Feature { get; set; }

        [Display(Name = "Kategori")]
        public string CategoryId { get; set; }

        [Display(Name = "Görsel")]
        public IFormFile PhotoFormFile { get; set; }
    }
}
