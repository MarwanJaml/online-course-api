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
        Task<CourseDetailModel?> GetCourseDetailAsync(int courseId); // Changed to nullable return
    }

    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<CourseModel>> GetAllCoursesAsync(int? categoryId = null)
        {
            try
            {
                var courses = await _courseRepository.GetAllCoursesAsync(categoryId);
                return courses ?? new List<CourseModel>(); // Return empty list instead of null
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses for category {CategoryId}", categoryId);
                throw; // Re-throw for controller to handle
            }
        }

        public async Task<CourseDetailModel?> GetCourseDetailAsync(int courseId)
        {
            if (courseId <= 0)
            {
                throw new ArgumentException("Invalid course ID", nameof(courseId));
            }

            try
            {
                var courseDetails = await _courseRepository.GetCourseDetailAsync(courseId);

                // Assuming repository returns List, take first or default
                return courseDetails?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for course {CourseId}", courseId);
                throw;
            }
        }
    }
}