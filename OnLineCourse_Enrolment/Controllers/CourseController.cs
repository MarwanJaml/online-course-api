using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnLineCourse.Core.Models;
using OnLineCourse.Data;
using OnLineCourse.Service;

namespace OnLineCourse_Enrolment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService courseService;

        public CourseController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CourseModel>>> GetAllCoursesAsync()
        {
            var courses = await courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpGet("Category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CourseModel>>> GetAllCoursesByCategoryIdAsync([FromRoute] int categoryId)
        {
            var courses = await courseService.GetAllCoursesAsync(categoryId);
            return Ok(courses);
        }

        // GET: api/Course/Detail/5
        [HttpGet("Detail/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourseDetailModel>> GetCourseDetailAsync(int courseId)
        {
            var courseDetail = await courseService.GetCourseDetailAsync(courseId);
            if (courseDetail == null)
            {
                return NotFound();
            }
            return Ok(courseDetail);
        }
    }
}
