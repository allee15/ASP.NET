using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int? ArticleId { get; set; }

        // incercam sa adaugam un nou camp 
        public int? GroupId { get; set; }

        public string? UserId { get; set; }

        // PASUL 6 - useri si roluri
        // un comentariu apartine unui singur utilizator 
        public virtual ApplicationUser? User { get; set; }

        public virtual Article? Article { get; set; }
        //public virtual Group? Group { get; set; }
    }

}
