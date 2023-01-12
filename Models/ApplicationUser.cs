using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "UserName-ul este obligatoriu")]
        [StringLength(50, ErrorMessage = "UserName-ul nu poate avea mai mult de 50 de caractere")]
        [MinLength(2, ErrorMessage = "UserName-ul trebuie sa aiba mai mult de 2 caractere")]
        public override string UserName { get; set; }
        public string? Nickname { get; set; }



        [Remote(action: "VerifyPrivacy", controller: "Users", ErrorMessage = "public/ private")]
       
        public string? Privacy { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Article>? Articles { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
        public virtual ICollection<UserInGroup>? UserInGroups { get; set; }
        public virtual ICollection<Group>? Groups { get; set; }
        public virtual ICollection<Friendship>?  Friendships { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }


    }
}
