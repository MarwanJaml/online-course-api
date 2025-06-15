using OnLineCourse.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnLineCourse.Data
{
    public interface ICourseRepository
    {
        Task<List<CourseModel>> GetAllCoursesAsync(int? catagoryId = null);
        Task<List<CourseDetailModel>> GetCourseDetailAsync(int courseId);
    }
}
