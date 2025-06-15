using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnLineCourse.Core.Models;

namespace OnLineCourse.Core.Entities
{
    [Table("CourseCategory")]
    public partial class CourseCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = null!;

        [StringLength(250)]
        public string? Description { get; set; }

        [InverseProperty("Category")]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        // Properly implemented conversion from Model to Entity
        public static implicit operator CourseCategory?(CourseCategoryModel? model)
        {
            if (model == null) return null;

            return new CourseCategory
            {
                CategoryId = model.CategoryId,
                CategoryName = model.CategoryName,
                Description = model.Description
            };
        }

        // Optional reverse conversion
        public static implicit operator CourseCategoryModel?(CourseCategory? entity)
        {
            if (entity == null) return null;

            return new CourseCategoryModel
            {
                CategoryId = entity.CategoryId,
                CategoryName = entity.CategoryName,
                Description = entity.Description
            };
        }
    }
}