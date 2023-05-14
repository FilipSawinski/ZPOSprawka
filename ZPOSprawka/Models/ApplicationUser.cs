using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ZPOSprawka.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual string Name { get; set; }
        public virtual string Surname { get; set; }

    }

    [NotMapped]
    public class UsersViewModel
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        [Display(Name = "Nazwa użytkownika")]
        public string UserName { get; set; }
        [Display(Name = "Imię")]
        public string Name { get; set; }
        [Display(Name = "Nazwisko")]
        public string Surname { get; set; }
        public Boolean isAdmin { get; set; }
        public Boolean isProwadzacy { get; set; }
    }

    [NotMapped]
    public class UserRolesViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool isSelected { get; set; }
    }
}
