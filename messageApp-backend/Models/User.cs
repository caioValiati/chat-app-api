using messageApp_backend.Models;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace messageApp_backend.models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[]? ProfilePicture { get; set; }

        public ICollection<User> Contacts { get; set; } = [];

        [JsonIgnore]
        public string? PasswordHash { get; set; }

        [JsonIgnore]
        public bool IsActive { get; set; }
    }
}
