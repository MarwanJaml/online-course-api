﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using OnLineCourse.Core.Models;
using OnLineCourse.Service;

namespace OnLineCourse_Enrolment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAdminController : ControllerBase
    {
        private readonly ICourseService courseService;

        public UserAdminController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        // GET: api/Course
        [HttpGet]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Read")]
        public async Task<ActionResult<List<UserModel>>> GetAllUsers()
        {
            var courses = await courseService.GetAllCoursesAsync();
            return Ok(courses);
        }
    }
}
