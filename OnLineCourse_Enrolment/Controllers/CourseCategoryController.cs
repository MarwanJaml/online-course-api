using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnLineCourse.Service;

namespace OnLineCourse_Enrolment.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CourseCategoryController : ControllerBase
    {

        private readonly ILogger<CourseCategoryController> _logger;
        private readonly ICourseCategoryService categoryService;

        public CourseCategoryController(ILogger<CourseCategoryController> logger, ICourseCategoryService categoryService)
        {
            _logger = logger;
            this.categoryService = categoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await categoryService.GetByIdAsync(id);
            //what if the id is not present?
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await categoryService.GetCourseCategories();
            return Ok(categories);
        }
    }
}
