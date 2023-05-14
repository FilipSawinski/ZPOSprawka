using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZPOSprawka.Data;
using ZPOSprawka.Models;

namespace ZPOSprawka.Controllers
{
    [Authorize(Roles = "Student, Administrator")]
    public class ExerciseController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment hostEnvironment;

        public ExerciseController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            this.userManager = userManager;
            this.hostEnvironment = hostEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            var currentuser = await userManager.FindByNameAsync(User.Identity.Name);
            var model = await _db.CourseGroups
                .Include(c => c.Leader).Include(c => c.StudentList)
                .Where(c => c.StudentList.Contains(currentuser)).ToListAsync();
            return View(model);
        }

        public async Task<ActionResult> GroupDetails(string id)
        {
            var groupfromid = await _db.CourseGroups.FindAsync(id);
            var model = await _db.Exercises.Include(c => c.ExerciseGroup)
                .Where(c => c.ExerciseGroup == groupfromid).ToListAsync();
            return View(model);
        }

        public async Task<ActionResult> ExerciseDetails(string id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var exercise = await _db.Exercises.Include(c => c.ExerciseGroup).Where(c => c.Id == id).FirstOrDefaultAsync();
            if(exercise==null)
            {
                return NotFound();
            }
            var currentuser = await userManager.GetUserAsync(User);
            var report = await _db.Reports
                .Include(c => c.Sender).Include(c => c.Exercise)
                .Where(c => c.Sender == currentuser && c.Exercise.Id == exercise.Id)
                .FirstOrDefaultAsync();
            var model = new ExerciseDetailsViewModel
            {
                Exercise = exercise,
                Report = report
            };
            return View(model);
        }
        public async Task<ActionResult> SendReport(string id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var exercise = await _db.Exercises.FindAsync(id);
            if(exercise==null)
            {
                return NotFound();
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendReport(SendReportViewModel model, string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var exercise = await _db.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                var fileName = Path.GetFileName(model.file.FileName);
                var newFileName = Guid.NewGuid().ToString();
                var path = Path.Combine(hostEnvironment.WebRootPath, "files", "reports", newFileName);
                using (Stream fileStream = new FileStream(path, FileMode.Create))
                {
                    await model.file.CopyToAsync(fileStream);
                }
                var currentuser = await userManager.GetUserAsync(User);
                var wasreportsentbefore = await _db.Reports.Include(c => c.Exercise)
                    .Where(c => c.Exercise.Id == exercise.Id).AnyAsync();
                if(wasreportsentbefore)
                {
                    var reporttodelete = await _db.Reports.Include(c => c.Exercise)
                        .Where(c => c.Exercise.Id == exercise.Id).FirstOrDefaultAsync();
                    System.IO.File.Delete(reporttodelete.FilePath);
                    _db.Reports.Remove(reporttodelete);
                }
                var report = new Report
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = fileName,
                    FilePath = path,
                    Exercise = exercise,
                    SendDate = DateTime.Now,
                    Sender = currentuser,
                    Status = "Wysłane"
                };
                await _db.Reports.AddAsync(report);
                await _db.SaveChangesAsync();
                return RedirectToAction("ExerciseDetails", new { id = exercise.Id });
            }
            return View(model);
        }

        public async Task<ActionResult> DownloadReport(string id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var reporttodownload = await _db.Reports.FindAsync(id);
            if(reporttodownload==null)
            {
                return NotFound();
            }
            string filePath = reporttodownload.FilePath;
            string fileName = reporttodownload.FileName;
            if(string.IsNullOrEmpty(filePath)||string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/force-download", fileName);
        }
    }
}
