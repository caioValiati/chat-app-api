using messageApp_backend.models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace messageApp_backend.Models
{
    public class Teste
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TesteName { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
