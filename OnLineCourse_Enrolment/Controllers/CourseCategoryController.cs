using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnLineCourse.Service;
using System;
using System.Threading.Tasks;

namespace OnlineCourse.Enrolment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CourseCategoryController : ControllerBase
    {
        private readonly ILogger<CourseCategoryController> _logger;
        private readonly ICourseCategoryService _categoryService;

        public CourseCategoryController(
            ILogger<CourseCategoryController> logger,
            ICourseCategoryService categoryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid ID requested: {Id}", id);
                    return BadRequest("ID must be a positive integer");
                }

                var category = await _categoryService.GetByIdAsync(id);

                if (category == null)
                {
                    _logger.LogInformation("Category with ID {Id} not found", id);
                    return NotFound($"Category with ID {id} not found");
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {Id}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetCourseCategories();

                if (categories == null || !categories.Any())
                {
                    _logger.LogInformation("No categories found");
                    return Ok(Enumerable.Empty<object>()); // Return empty array instead of null
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }
    }
}