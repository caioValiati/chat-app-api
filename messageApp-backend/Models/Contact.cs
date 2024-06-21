using messageApp_backend.models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace messageApp_backend.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int ContactUserId { get; set; }
        public int? LastMessageId { get; set; }

        public virtual User ContactUser { get; set; }
        public virtual Message? LastMessage { get; set; }
    }
}

