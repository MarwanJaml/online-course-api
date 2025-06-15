using Microsoft.EntityFrameworkCore;
using OnLineCourse.Core.Entities;  // This ensures OnlineCourseDbContext is found
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnLineCourse.Data
{
    public class CourseCategoryRepository : ICourseCategoryRepository
    {
        private readonly OnlineCourseDbContext _context;

        public CourseCategoryRepository(OnlineCourseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CourseCategory?> GetByIdAsync(int id)
        {
            return await _context.CourseCategories
                .AsNoTracking()  // Recommended for read-only operations
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<List<CourseCategory>> GetCourseCategories()
        {
            return await _context.CourseCategories
                .AsNoTracking()  // Better performance for read-only
                .ToListAsync();
        }
    }
}