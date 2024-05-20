namespace messageApp_backend.Models
{
    public class Message
    {
        public int id { get; set; }
        public int usuarioRemetente { get; set; }
        public int usuarioDestinatario { get; set; }
        public DateTime dataEnvio { get; set; }
        public string conteudo { get; set; }
    }
}
