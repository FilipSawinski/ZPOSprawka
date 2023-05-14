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
    public class Report
    {
        [Key]
        public virtual string Id { get; set; }
        [Required]
        public virtual string FilePath { get; set; }
        [Required]
        public virtual string FileName { get; set; }
        [Required]
        public virtual ApplicationUser Sender { get; set; }
        [Required]
        public virtual DateTime SendDate { get; set; }
        public virtual Exercise Exercise { get; set; }
        [Range(2, 5)]
        public virtual int Grade { get; set; }
        public virtual string Status { get; set; }
    }

    [NotMapped]
    public class SendReportViewModel
    {
        [MaxFileSize(20 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".rar", ".pdf", ".docx" })]
        public virtual IFormFile file { get; set; }
    }

    [NotMapped]
    public class GradeReportViewModel
    {
        [Range(2,6)]
        public virtual int Grade { get; set; }
        [Required]
        public virtual bool isToCorrect { get; set; }
        public virtual string exerciseId { get; set; }
    }
}
