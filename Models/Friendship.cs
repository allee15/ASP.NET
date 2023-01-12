

using System.ComponentModel.DataAnnotations;


namespace ArticlesApp.Models
{
    public class Friendship
    {
        [Key]
        public int FriendshipId { get; set; }

        public DateTime DateJoined { get; set; }
        public DateTime? DateAccepted { get; set; }

        public string SenderUserId { get; set; }

        public string ReceiverUserId { get; set; }
        // 0 - este in asteptare
        // 1 - acceptat
        // 2 - declined
        public int StatusCerere { get; set; }
        public virtual ApplicationUser? User { get; set; }
        //public virtual FriendRequest FriendReq { get; set; }
        // 0 - nu sunt prieteni
        // 1 - sunt prieteni
        //public bool IsFriendship { get; set; }


    }
}
