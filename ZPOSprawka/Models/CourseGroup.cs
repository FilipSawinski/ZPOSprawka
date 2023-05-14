using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using ZPOSprawka.Utilities;

namespace ZPOSprawka.Models
{
    public class CourseGroup
    {
        [Key]
        public virtual string Id { get; set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual List<ApplicationUser> StudentList { get; set; }
        [Required]
        public virtual bool isActive { get; set; }
        [Required]
        public virtual ApplicationUser Leader { get; set; }

    }

    [NotMapped]
    public class AddGroupViewModel
    {
        [Required]
        public virtual string Name { get; set; }
    }

    [NotMapped]
    public class GroupDetailsViewModel
    {
        public virtual string Name { get; set; }
        public virtual ApplicationUser Leader { get; set; }
        public virtual bool isActive { get; set; }
        public virtual List<ApplicationUser> AttendantList { get; set; }
        public virtual List<Exercise> ExerciseList { get; set; }

    }

    [NotMapped]
    public class AddGroupFromFileViewModel
    {
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual string Password { get; set; }
        public virtual ApplicationUser Leader { get; set; }
        public virtual bool isActive { get; set; }
        public virtual List<ApplicationUser> AttendantList { get; set; }
        [AllowedExtensions(new string[] { ".csv" })]
        [MaxFileSize(5*1024*1024)]
        [Required]
        public IFormFile list { get; set; }
    }
}
