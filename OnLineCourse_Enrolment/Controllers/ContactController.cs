using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnLineCourse.Core.Models;
using OnLineCourse.Service;

namespace OnLineCourse_Enrolment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ContactController : ControllerBase
    {
        private readonly IEmailNotification emailNotification;

        public ContactController(IEmailNotification emailNotification)
        {
            this.emailNotification = emailNotification;
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessage contactMessage)
        {
            if (contactMessage == null)
            {
                return BadRequest("Contact message cannot be null.");
            }

            await emailNotification.SendEmailForContactUs(contactMessage);

            return Ok(new { message = "Message sent successfully!", model = contactMessage });
        }
    }



}
