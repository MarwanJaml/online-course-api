namespace OnLineCourse_Enrolment.Model
{
    public class UpdateUserProfileModel
    {
        public required int UserId { get; set; }
        public string? Bio { get; set; }
        public IFormFile? Picture { get; set; }
    }
}
