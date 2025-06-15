using OnLineCourse.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnLineCourse.Core.Models

{

    public class CourseDetailModel : CourseModel
    {
        public List<UserReviewModel> Reviews { get; set; } = new List<UserReviewModel>();
        public List<SessionDetailModel> SessionDetails { get; set; } = new List<SessionDetailModel>();
    }

    public class UserReviewModel
    {
        public int ReviewId { get; set; }

        public int CourseId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }

        public string? Comments { get; set; }
    }

    public class SessionDetailModel
    {
        public int SessionId { get; set; }

        public int CourseId { get; set; }

        [StringLength(100)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [StringLength(500)]
        public string? VideoUrl { get; set; }

        public int VideoOrder { get; set; }
    }
    public class CourseModel
    {
        
    [Key]
        public int CourseId { get; set; }

        [StringLength(100)]
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [StringLength(10)]
        public string CourseType { get; set; } = null!;

        public int? SeatsAvailable { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Duration { get; set; }

        public int CategoryId { get; set; }

        public int InstructorId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string? Thumbnail { get; set; }

        [ForeignKey("CategoryId")]
        [InverseProperty("Courses")]
        public virtual CourseCategory Category { get; set; } = null!;

        public UserReviewModel userReview { get; set; }

        [InverseProperty("Course")]
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        [ForeignKey("InstructorId")]
        [InverseProperty("Courses")]
        public virtual Instructor Instructor { get; set; } = null!;

        [InverseProperty("Course")]
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        [InverseProperty("Course")]
        public virtual ICollection<SessionDetail> SessionDetails { get; set; } = new List<SessionDetail>();
    }

}

