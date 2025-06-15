using OnLineCourse.Core.Entities;
using OnLineCourse.Core.Models;
using OnLineCourse.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnLineCourse.Service
{
    public interface ICourseCategoryService
    {
        Task<CourseCategoryModel?> GetByIdAsync(int id);
        Task<List<CourseCategoryModel>> GetCourseCategories();
    }

    public class CourseCategoryService : ICourseCategoryService
    {
        private readonly ICourseCategoryRepository _categoryRepository; // Fixed spelling

        public CourseCategoryService(ICourseCategoryRepository categoryRepository) // Fixed parameter name
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CourseCategoryModel?> GetByIdAsync(int id)
        {
            var data = await _categoryRepository.GetByIdAsync(id);
            if (data == null) return null;

            return new CourseCategoryModel
            {
                CategoryId = data.CategoryId,
                CategoryName = data.CategoryName,
                Description = data.Description
            };
        }

        public async Task<List<CourseCategoryModel>> GetCourseCategories()
        {
            var data = await _categoryRepository.GetCourseCategories();
            return data.Select(c => new CourseCategoryModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description
            }).ToList();
        }
    }
}