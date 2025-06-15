using OnLineCourse.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnLineCourse.Data
{
    public interface ICourseCategoryRepository
    {
        Task<CourseCategory?> GetByIdAsync(int id);
        Task<List<CourseCategory>> GetCourseCategories();
    }
}