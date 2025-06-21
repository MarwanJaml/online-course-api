using Microsoft.EntityFrameworkCore;
using OnLineCourse.Core.Entities;  // For entity classes
using OnLineCourse.Core.Models;    // For model classes
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnLineCourse.Data
{
    public class CourseRepository : ICourseRepository
    {
        private readonly OnLineCourse.Core.Entities.OnlineCourseDbContext _dbContext;

        public CourseRepository(OnLineCourse.Core.Entities.OnlineCourseDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<CourseModel>> GetAllCoursesAsync(int? categoryId = null)
        {
            var query = _dbContext.Courses
                .Include(c => c.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            return await query
                .Select(c => new CourseModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    CourseType = c.CourseType,
                    SeatsAvailable = c.SeatsAvailable,
                    Duration = c.Duration,
                    CategoryId = c.CategoryId,
                    InstructorId = c.InstructorId,
                    Thumbnail = c.Thumbnail,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Category = new CourseCategoryModel
                    {
                        CategoryId = c.Category.CategoryId,
                        CategoryName = c.Category.CategoryName,
                        Description = c.Category.Description
                    }
                })
                .AsNoTracking()
                .ToListAsync();
        }

        // Fixed method: Changed parameter from categoryId to courseId and return type to single CourseDetailModel
        public async Task<CourseDetailModel> GetCourseDetailAsync(int courseId)
        {
            return await _dbContext.Courses
                .Include(c => c.Category)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .Include(c => c.SessionDetails)
                .Where(c => c.CourseId == courseId) // Changed from CategoryId to CourseId
                .Select(c => new CourseDetailModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    CourseType = c.CourseType,
                    SeatsAvailable = c.SeatsAvailable,
                    Duration = c.Duration,
                    CategoryId = c.CategoryId,
                    InstructorId = c.InstructorId,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Thumbnail = c.Thumbnail,
                    Category = new CourseCategoryModel
                    {
                        CategoryId = c.Category.CategoryId,
                        CategoryName = c.Category.CategoryName,
                        Description = c.Category.Description
                    },
                    Reviews = c.Reviews.Select(r => new UserReviewModel
                    {
                        CourseId = r.CourseId,
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        Rating = r.Rating,
                        Comments = r.Comments,
                        ReviewDate = r.ReviewDate
                    })
                    .OrderByDescending(r => r.Rating)
                    .Take(10)
                    .ToList(),
                    SessionDetails = c.SessionDetails.Select(sd => new SessionDetailModel
                    {
                        SessionId = sd.SessionId,
                        CourseId = sd.CourseId,
                        Title = sd.Title,
                        Description = sd.Description,
                        VideoUrl = sd.VideoUrl,
                        VideoOrder = sd.VideoOrder
                    })
                    .OrderBy(sd => sd.VideoOrder)
                    .ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(); // Changed from ToListAsync() to FirstOrDefaultAsync()
        }

        // Optional: If you still need a method to get courses by category, add this separate method
        public async Task<List<CourseDetailModel>> GetCourseDetailsByCategoryAsync(int categoryId)
        {
            return await _dbContext.Courses
                .Include(c => c.Category)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .Include(c => c.SessionDetails)
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new CourseDetailModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    CourseType = c.CourseType,
                    SeatsAvailable = c.SeatsAvailable,
                    Duration = c.Duration,
                    CategoryId = c.CategoryId,
                    InstructorId = c.InstructorId,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Thumbnail = c.Thumbnail,
                    Category = new CourseCategoryModel
                    {
                        CategoryId = c.Category.CategoryId,
                        CategoryName = c.Category.CategoryName,
                        Description = c.Category.Description
                    },
                    Reviews = c.Reviews.Select(r => new UserReviewModel
                    {
                        CourseId = r.CourseId,
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        Rating = r.Rating,
                        Comments = r.Comments,
                        ReviewDate = r.ReviewDate
                    })
                    .OrderByDescending(r => r.Rating)
                    .Take(10)
                    .ToList(),
                    SessionDetails = c.SessionDetails.Select(sd => new SessionDetailModel
                    {
                        SessionId = sd.SessionId,
                        CourseId = sd.CourseId,
                        Title = sd.Title,
                        Description = sd.Description,
                        VideoUrl = sd.VideoUrl,
                        VideoOrder = sd.VideoOrder
                    })
                    .OrderBy(sd => sd.VideoOrder)
                    .ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}