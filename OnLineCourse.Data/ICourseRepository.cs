using OnLineCourse.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnLineCourse.Data
{
    public interface ICourseRepository
    {
        Task<List<CourseModel>> GetAllCoursesAsync(int? categoryId = null);
        Task<CourseDetailModel> GetCourseDetailAsync(int courseId);
    }
}