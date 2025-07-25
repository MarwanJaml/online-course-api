using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnLineCourse.Core.Models;
using OnLineCourse.Service;
using OnLineCourse_Enrolment.Model;
using Azure.Storage.Blobs;

namespace OnLineCourse_Enrolment.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class UserProfileController : ControllerBase
    {
        private readonly IAzureBlobStorageService _blobStorageService;
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IAzureBlobStorageService blobStorageService, IUserProfileService userProfileService)
        {
            _blobStorageService = blobStorageService;
            _userProfileService = userProfileService;
        }

        [HttpGet("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserProfile([FromRoute] int id)
        {
            var userInfo = await _userProfileService.GetUserInfoAsync(id);
            if (userInfo == null)
            {
                return NotFound();
            }
            return Ok(userInfo);
        }

        [HttpPost("updateProfile")]
        //[Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateUserProfileModel model)
        {
            string pictureUrl = null;

            if (model.Picture != null)
            {
                using (var stream = new MemoryStream())
                {
                    await model.Picture.CopyToAsync(stream);

                    // Upload the byte array or stream to Azure Blob Storage
                    pictureUrl = await _blobStorageService.UploadAsync(stream.ToArray(),
                        $"{model.UserId}_profile_picture.{model.Picture.FileName.Split('.').LastOrDefault()}");
                }

                // Update the profile picture URL in the database
                await _userProfileService.UpdateUserProfilePicture(model.UserId, pictureUrl);
            }

            // Update bio
            if (model.Bio != null)
            {
                await _userProfileService.UpdateUserBio(model.UserId, model.Bio);
            }

            return Ok(model);
        }
    }


}
