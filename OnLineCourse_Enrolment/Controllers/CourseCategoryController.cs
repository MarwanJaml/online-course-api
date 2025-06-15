using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnLineCourse.Service;

namespace OnLineCourse_Enrolment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseCategoryController : ControllerBase
    {
        private readonly ICourseCategoryService courseCategoryService;

        public CourseCategoryController(ICourseCategoryService courseCategoryService)
        {
            this.courseCategoryService = courseCategoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id )
        {
            //when useing .Result thats mean sync not async

            var data = await courseCategoryService.GetCourseCategories();
            if (data == null )
            {
                return NotFound("No course categories found.");
            }
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            //when we the list is returned without any spesific resourse , the not found not applicable

            var categories = await courseCategoryService.GetCourseCategories();
          
            return Ok(categories);
        }
    }
}
