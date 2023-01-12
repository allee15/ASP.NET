using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        // grupul are un nume 
        [Required(ErrorMessage = "Numele grupului este obligatoriu")]
        public string Name { get; set; }
        [Required(ErrorMessage ="Categoria este obligatorie")]
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }
       public virtual ICollection<Article>? Articles { get; set; }
        
        // fiecarui grup ii revin o colectie de comentarii
        public virtual ICollection<Comment>? Comments { get; set; }
        // fiecarui grup ii revine si o colectie de useri
        
        public string? UserId { get; set; }
        // PASUL 6 - useri si roluri
        // un articol apartine unui singur utilizator 
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<UserInGroup>? UserInGroups { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? UserList { get; set; }


    }
}
