using System;
using System.ComponentModel.DataAnnotations;

namespace OnLineCourse.Core.Models
{
    public class CourseCategoryModel
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = null!;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}