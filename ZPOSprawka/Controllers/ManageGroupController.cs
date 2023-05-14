using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZPOSprawka.Data;
using ZPOSprawka.Models;

namespace ZPOSprawka.Controllers
{
    [Authorize(Roles = "Administrator, Prowadzący")]
    public class ManageGroupController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment hostEnvironment;

        public ManageGroupController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            this.userManager = userManager;
            this.hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser currentUser = await userManager.GetUserAsync(User);
            //Pobranie wszystkich grup aktualnie zalogowanego użytkownika
            var model = await _db.CourseGroups
                .Include(c => c.Leader).Include(c => c.StudentList)
                .Where(c => c.Leader==currentUser).ToListAsync();
            return View(model);
        }

        public ActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateGroup(AddGroupViewModel model)
        {
            if(ModelState.IsValid)
            {
                //mapowanie grupy na grupe z uzupełnionym info
                var grouptoadd = new CourseGroup
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    isActive = true,
                    Leader = await userManager.FindByNameAsync(User.Identity.Name)
                };
                await _db.CourseGroups.AddAsync(grouptoadd);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult CreateGroupFromFile()
        {
            var model = new AddGroupFromFileViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CreateGroupFromFile(AddGroupFromFileViewModel model)
        {
            if(ModelState.IsValid)
            {
                //tworzenie grupy
                var grouptoadd = new CourseGroup
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Leader = await userManager.FindByNameAsync(User.Identity.Name),
                    isActive = true,
                    StudentList = new List<ApplicationUser>()
                };
                //pobranie emaili z pliku do tablicy "result"
                var file = model.list;
                var result = new List<string>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        result.Add(reader.ReadLine().Replace(";", ""));
                }
                //utworzenie kont dla użytkowników którzy jeszcze nie istnieją
                foreach(var u in result)
                {
                    var potentialuser = await userManager.FindByNameAsync(u);
                    //jeśli nie istnieje
                    if(potentialuser == null)
                    {
                        var utoadd = new ApplicationUser
                        {
                            UserName = u,
                            Email = u
                        };
                        await userManager.CreateAsync(utoadd, model.Password);
                        
                    }
                    potentialuser = await userManager.FindByEmailAsync(u);
                    //dodanie studenta do grupy
                    await userManager.AddToRoleAsync(potentialuser, "Student");
                    grouptoadd.StudentList.Add(potentialuser);
                }
                await _db.CourseGroups.AddAsync(grouptoadd);
                await _db.SaveChangesAsync();
                
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<ActionResult> Details(string id)
        {
            //gdzieś to kurcze widziałem
            if(id==null)
            {
                return NotFound();
            }
            var group = await _db.CourseGroups.Include(c=>c.Leader).FirstOrDefaultAsync(c=>c.Id==id);
            if(group==null)
            {
                return NotFound();
            }
            //pobranie studentów którzy są w podanej grupie do listy
            var students = userManager.Users.ToList();
            var model = new GroupDetailsViewModel();
            model.AttendantList = new List<ApplicationUser>();
            foreach(var s in students)
            {
                if(_db.CourseGroups.Include(c=>c.StudentList).Where(c=>c.Id==id).Any(c=>c.StudentList.Contains(s)))
                {
                    model.AttendantList.Add(s);
                }
            }
            model.isActive = group.isActive;
            model.Name = group.Name;
            model.Leader = group.Leader;
            model.ExerciseList = await _db.Exercises.Where(c => c.ExerciseGroup == group).ToListAsync();
            return View(model);
        }

        public async Task<ActionResult> AddStudentToGroup(string id)
        {
            //co to
            if (id == null)
            {
                return NotFound();
            }
            var group = await _db.CourseGroups.Include(c => c.StudentList).FirstOrDefaultAsync(c => c.Id == id);
            if (group == null)
            {
                return NotFound();
            }
            //pobierz studentów którzy nie są w danej grupie do listy
            var model = userManager.Users
                .Where(c => !group.StudentList.Contains(c))  
                .ToList();
            //zweryfikuj użytkowników czy są studentami
            foreach(var s in model.ToList())
            {
                var res = await userManager.IsInRoleAsync(s, "Student");
                if(!res)
                {
                    model.Remove(s);
                }
            }
            return View(model);
        }

        public async Task<ActionResult> SelectStudentToGroup(string studentid, string groupid)
        {
            //hmm
            if(studentid==null||groupid==null)
            {
                return NotFound();
            }
            var studenttoadd = await userManager.FindByIdAsync(studentid);
            var grouptoupdate = await _db.CourseGroups
                .Include(c=>c.StudentList).Include(c=>c.Leader)
                .FirstOrDefaultAsync(c=>c.Id==groupid);
            if(studenttoadd==null||grouptoupdate==null)
            {
                return NotFound();
            }
            //dodaj studenta do listy studentów w grupie tylko jeśli go tam jeszcze nie ma
            if(!grouptoupdate.StudentList.Contains(studenttoadd))
            {
                grouptoupdate.StudentList.Add(studenttoadd);
                _db.Update(grouptoupdate);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("AddStudentToGroup", new { id = groupid });
        }

        public async Task<ActionResult> RemoveStudentFromGroup(string studentid, string groupid)
        {
            if (studentid == null || groupid == null)
            {
                return NotFound();
            }
            var studenttodelete = await userManager.FindByIdAsync(studentid);
            var grouptoupdate = await _db.CourseGroups
                .Include(c => c.StudentList).Include(c => c.Leader)
                .FirstOrDefaultAsync(c => c.Id == groupid);
            if (studenttodelete == null || grouptoupdate == null)
            {
                return NotFound();
            }
            //jeżeli grupa nie ma tego studenta no to coś poszło nie tak
            if(!grouptoupdate.StudentList.Contains(studenttodelete))
            {
                return BadRequest();
            }
            grouptoupdate.StudentList.Remove(studenttodelete);
            _db.Update(grouptoupdate);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = groupid });
        }


        public async Task<ActionResult> CloseGroup(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var model = await _db.CourseGroups.Include(c=>c.Leader).FirstOrDefaultAsync(c=>c.Id==id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("CloseGroup")]
        public async Task<ActionResult> CloseGroupConfirmed(string id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var grouptoclose = await _db.CourseGroups.Include(c=>c.StudentList).FirstOrDefaultAsync(c=>c.Id==id);
            if(grouptoclose==null)
            {
                return NotFound();
            }
            grouptoclose.isActive = false;
            grouptoclose.StudentList.Clear();
            _db.CourseGroups.Update(grouptoclose);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteGroup(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var model = await _db.CourseGroups.Include(c => c.Leader).FirstOrDefaultAsync(c => c.Id == id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("DeleteGroup")]
        public async Task<ActionResult> DeleteGroupConfirmed(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var grouptodelete = await _db.CourseGroups.Include(c => c.StudentList).FirstOrDefaultAsync(c => c.Id == id);
            if (grouptodelete == null)
            {
                return NotFound();
            }
            var exercisestodelete = await _db.Exercises.Include(c => c.ExerciseGroup).Where(c => c.ExerciseGroup == grouptodelete).ToListAsync();
            foreach(var e in exercisestodelete)
            {
                var reportstodelete = await _db.Reports.Include(c => c.Exercise).Where(c => c.Exercise == e).ToListAsync();
                foreach(var v in reportstodelete)
                {
                    _db.Reports.Remove(v);
                }
                _db.Exercises.Remove(e);
            }
            _db.CourseGroups.Remove(grouptodelete);           
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> AddExercise(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var group = await _db.CourseGroups.FirstOrDefaultAsync(c => c.Id == id);
            if (group == null)
            {
                return NotFound();
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddExercise(AddExerciseViewModel model, string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return NotFound();
                }
                var group = await _db.CourseGroups.FirstOrDefaultAsync(c => c.Id == id);
                if (group == null)
                {
                    return NotFound();
                }
                var exercisetoadd = new Exercise
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Description = model.Description,
                    ExerciseGroup = group
                };
                await _db.Exercises.AddAsync(exercisetoadd);
                await _db.SaveChangesAsync();
                var groupid = id;
                return RedirectToAction("Details", new { id = groupid });
            }
            return View(model);
        }

        public async Task<ActionResult> DeleteExercise(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var exercisetodelete = await _db.Exercises.Include(c=>c.ExerciseGroup).Where(c => c.Id == id).FirstOrDefaultAsync();
            if (exercisetodelete==null)
            {
                return NotFound();
            }
            var groupid = exercisetodelete.ExerciseGroup.Id;
            var reportstodelete = await _db.Reports.Include(c => c.Exercise).Where(c => c.Exercise == exercisetodelete).ToListAsync();
            foreach(var r in reportstodelete)
            {
                System.IO.File.Delete(r.FilePath);
                _db.Reports.Remove(r);
            }
            _db.Exercises.Remove(exercisetodelete);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = groupid });
        }

        public async Task<ActionResult> ExerciseDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var exercise = await _db.Exercises.Include(c => c.ExerciseGroup).Where(c => c.Id == id).FirstOrDefaultAsync();
            if (exercise == null)
            {
                return NotFound();
            }
            var reports = await _db.Reports.Include(c => c.Exercise).Include(c=>c.Sender).Where(c => c.Exercise == exercise).ToListAsync();
            return View(reports);
        }

        public async Task<ActionResult> ViewReport(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var report = await _db.Reports.Include(c=>c.Exercise).FirstOrDefaultAsync(c=>c.Id==id);
            if(report==null)
            {
                return NotFound();
            }
            return View(report);
        }

        public async Task<ActionResult> GradeReport(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var report = await _db.Reports.Include(c=>c.Exercise).FirstOrDefaultAsync(c => c.Id == id);
            if (report == null)
            {
                return NotFound();
            }
            var model = new GradeReportViewModel
            {
                exerciseId = report.Exercise.Id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> GradeReport(GradeReportViewModel model, string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var report = await _db.Reports.Include(c=>c.Exercise).Include(c=>c.Sender).FirstOrDefaultAsync(c => c.Id == id);
            if (report == null)
            {
                return NotFound();
            }
            report.Grade = model.Grade;
            if(model.isToCorrect)
            {
                report.Status = "Do poprawy";
            } else
            {
                report.Status = "Ocenione";
            }
            _db.Reports.Update(report);
            await _db.SaveChangesAsync();
            return RedirectToAction("ViewReport", new { id = report.Id });

        }
    }
}
