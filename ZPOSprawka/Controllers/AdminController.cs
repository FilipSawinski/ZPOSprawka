using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZPOSprawka.Data;
using ZPOSprawka.Models;

namespace ZPOSprawka.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext _db;

        
        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext _db)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._db = _db;
        }

        //admin/
        public IActionResult Index()
        {
            return View();
        }

        //admin/users/
        public async Task<ActionResult> Users()
        {
            //użytkownicy do listy i mapowanie ich na klasę UsersViewModel
            var users = userManager.Users.ToList();
            List<UsersViewModel> model = new List<UsersViewModel>();
            foreach (var u in users)
            {
                UsersViewModel vm = new UsersViewModel();
                vm.Email = u.Email;
                vm.Id = u.Id;
                vm.Name = u.Name;
                vm.Surname = u.Surname;
                vm.isAdmin = userManager.IsInRoleAsync(u, "Administrator").Result;
                vm.isProwadzacy = userManager.IsInRoleAsync(u, "Prowadzący").Result;
                model.Add(vm);
            }
            return View(model);
        }

        //admin/edituser/{id}
        public async Task<ActionResult> Edituser(string id)
        {
            //sprawdzenie czy id jest ok
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            //mapowanie użytkownika na UsersViewModel
            var model = new UsersViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                isAdmin = await userManager.IsInRoleAsync(user, "Administrator")
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edituser(UsersViewModel user)
        {
            if (ModelState.IsValid)
            {
                //przemapowanie UsersViewModel na ApplicationUser żeby go dodać do bazy
                var usertoupdate = await userManager.FindByIdAsync(user.Id);
                usertoupdate.Email = user.Email;
                usertoupdate.UserName = user.Email;
                usertoupdate.Name = user.Name;
                usertoupdate.Surname = user.Surname;
                await userManager.UpdateAsync(usertoupdate);
                return RedirectToAction("Users");
            }
            return View(user);
        }

        //admin/deleteuser/{id}
        public async Task<ActionResult> Deleteuser(string id)
        {
            //ty no ja nw
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var model = new UsersViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname
            };
            return View(model);
        }

        [HttpPost, ActionName("Deleteuser")]
        public async Task<ActionResult> DeleteuserConfirmed(string Id)
        {
            //ty no ja nw
            if (Id == null)
            {
                return BadRequest();
            }
            var usertodelete = await userManager.FindByIdAsync(Id);
            if (usertodelete == null)
            {
                return NotFound();
            }
            await userManager.DeleteAsync(usertodelete);
            return RedirectToAction("Users");
        }

        //admin/manageroles/{id}
        public async Task<ActionResult> Manageroles(string id)
        {
            //ciekawe
            if (id == null)
            {
                return NotFound();
            }
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            //przejechanie po każdej roli i dodanie jej do listy
            ViewBag.user = user;
            var model = new List<UserRolesViewModel>();
            foreach (var role in roleManager.Roles)
            {
                var modelelement = new UserRolesViewModel();
                modelelement.RoleId = role.Id;
                modelelement.RoleName = role.Name;
                //jeżeli użytkownik ma role, no to jak wejdzie to ma zaznaczone, jak nie to nie
                if (userManager.IsInRoleAsync(user, role.Name).Result)
                {
                    modelelement.isSelected = true;
                }
                else
                {
                    modelelement.isSelected = false;
                }
                model.Add(modelelement);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Manageroles(List<UserRolesViewModel> roleslist, string id)
        {
            //ty no ja nw
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            //wywalenie użytkownika ze wszystkich roli i wrzucenie go do tych które są zaznaczone
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                await userManager.RemoveFromRoleAsync(user, role);
            }
            foreach (var role in roleslist)
            {
                if (role.isSelected)
                {
                    await userManager.AddToRoleAsync(user, role.RoleName);
                }
            }
            return RedirectToAction("Edituser", "Admin", new { id = user.Id });
        }
    }
}
