using messageApp_backend.models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static messageApp_backend.Models.MessageStatusAttribute;

namespace messageApp_backend.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageStatusAttribute : Attribute
    {
        private MessageStatus sending;

        public MessageStatusAttribute(MessageStatus sending)
        {
            this.sending = sending;
        }

        public MessageStatus Status { get; set; }

        public enum MessageStatus
        {
            Sended,
            Received,
            Visualized
        };
    }

    [MessageStatusAttribute(MessageStatus.Sended)]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UsuarioRemetenteId { get; set; }
        public int UsuarioDestinatarioId { get; set; }
        public DateTime DataEnvio { get; set; }
        public string Conteudo { get; set; }
        public MessageStatus Status { get; set; }
        public byte[]? Image { get; set; }
    }
}
