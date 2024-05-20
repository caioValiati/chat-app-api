using System.Text.Json.Serialization;

namespace messageApp_backend.models
{
    public class User
    {
        public int id { get; set; }
        public string userName { get; set; }

        [JsonIgnore]
        public string passwordHash { get; set; }

        [JsonIgnore]
        public bool isActive { get; set; }
    }
}
