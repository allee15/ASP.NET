using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class UserInGroup
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int GroupId { get; set; }    

        public virtual ApplicationUser? User { get; set; }

        public virtual Group? Group { get; set; }   


    }
}
