using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ZPOSprawka.Models;

namespace ZPOSprawka.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CourseGroup> CourseGroups { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Report> Reports { get; set; }

    }
}
