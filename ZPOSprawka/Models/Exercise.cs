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
    public class Exercise
    {
        [Key]
        public virtual string Id { get; set; }
        [Required]
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        [Required]
        public virtual CourseGroup ExerciseGroup { get; set; }
    }

    [NotMapped]
    public class AddExerciseViewModel
    {
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual string Description { get; set; }
    }

    [NotMapped]
    public class ExerciseDetailsViewModel
    {
        public virtual Exercise Exercise { get; set; }
        public virtual Report Report { get; set; }
    }

    
}
