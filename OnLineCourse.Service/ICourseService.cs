using OnLineCourse.Core.Entities;
using OnLineCourse.Core.Models;
using OnLineCourse.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OnLineCourse.Service
{
    public interface ICourseService
    {
        Task<List<CourseModel>> GetAllCoursesAsync(int? categoryId = null);
        Task<CourseDetailModel> GetCourseDetailAsync(int courseId);
    }
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            this.courseRepository = courseRepository;
        }

        public Task<List<CourseModel>> GetAllCoursesAsync(int? categoryId = null)
        {
            return courseRepository.GetAllCoursesAsync(categoryId);
        }


        public Task<CourseDetailModel> GetCourseDetailAsync(int courseId)
        {
            return courseRepository.GetCourseDetailAsync(courseId);
        }
    }
}