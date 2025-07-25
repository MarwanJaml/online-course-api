using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using OnLineCourse.Core.Models;
using OnLineCourse.Service;
using OnLineCourse_Enrolment.Common;

namespace OnLineCourse_Enrolment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Comment out [Authorize] for development testing
    // [Authorize]
    public class VideoRequestController : ControllerBase
    {
        private readonly IVideoRequestService _videoRequestService;
        private readonly IUserClaims userClaims;

        public VideoRequestController(IVideoRequestService videoRequestService, IUserClaims userClaims)
        {
            _videoRequestService = videoRequestService;
            this.userClaims = userClaims;
        }

        [HttpGet]
        // Comment out RequiredScope for development testing
        // [RequiredScope("User.Read")]
        public async Task<ActionResult<IEnumerable<VideoRequestModel>>> GetAll()
        {
            List<VideoRequestModel> videoRequests;

            // For development, you might want to hardcode user roles or make this conditional
            var userRoles = userClaims.GetUserRoles();
            if (userRoles.Contains("Admin"))
            {
                videoRequests = await _videoRequestService.GetAllAsync();
            }
            else
            {
                var videoRequest = await _videoRequestService.GetByUserIdAsync(userClaims.GetUserId());
                videoRequests = videoRequest.ToList();
            }
            return Ok(videoRequests);
        }

        [HttpGet("{id}")]
        // [RequiredScope("User.Read")]
        public async Task<ActionResult<VideoRequestModel>> GetById(int id)
        {
            var videoRequest = await _videoRequestService.GetByIdAsync(id);
            if (videoRequest == null)
            {
                return NotFound();
            }
            return Ok(videoRequest);
        }

        [HttpGet("user/{userId}")]
        // [RequiredScope("User.Read")]
        public async Task<ActionResult<IEnumerable<VideoRequestModel>>> GetByUserId(int userId)
        {
            var videoRequests = await _videoRequestService.GetByUserIdAsync(userId);
            return Ok(videoRequests);
        }

        [HttpPost]
        // [RequiredScope("User.Write")]
        public async Task<ActionResult<VideoRequestModel>> Create(VideoRequestModel model)
        {
            var createdVideoRequest = await _videoRequestService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = createdVideoRequest.VideoRequestId }, createdVideoRequest);
        }

        [HttpPut("{id}")]
        // [RequiredScope("User.Write")]
        public async Task<IActionResult> Update(int id, VideoRequestModel model)
        {
            try
            {
                var updatedVideoRequest = await _videoRequestService.UpdateAsync(id, model);
                return Ok(updatedVideoRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        // [RequiredScope("User.Write")]
        public async Task<IActionResult> Delete(int id)
        {
            await _videoRequestService.DeleteAsync(id);
            return NoContent();
        }
    }
}