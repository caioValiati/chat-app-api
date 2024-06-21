using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using messageApp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using messageApp_backend.models;

namespace messageApp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageContext _context;
        private readonly UserContext _userContext;

        public MessagesController(MessageContext context, UserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet("{userId1}/{userId2}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(int userId1, int userId2)
        {
            var messages = await _context.Messages
                .Where(m => (m.UsuarioDestinatarioId == userId1 && m.UsuarioRemetenteId == userId2) ||
                            (m.UsuarioDestinatarioId == userId2 && m.UsuarioRemetenteId == userId1))
                .ToListAsync();

            
            return Ok(messages);
        }

        public class UserWithMessage
        {
            public Message Message { get; set; }
            public User User { get; set; }
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserWithMessage>>> GetLastMessages(int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.UsuarioDestinatarioId == userId || m.UsuarioRemetenteId == userId)
                .GroupBy(m => new { m.UsuarioDestinatarioId, m.UsuarioRemetenteId })
                .Select(group => new UserWithMessage
                {
                    Message = group.OrderByDescending(m => m.DataEnvio).FirstOrDefault(),
                    User = GetUserById(group.Key.UsuarioDestinatarioId == userId ? group.Key.UsuarioRemetenteId : group.Key.UsuarioDestinatarioId)
                })
                .ToListAsync();

            return Ok(messages);
        }

        private User GetUserById(int userId)
        {
            return _userContext.Users.Find(userId);
        }
    }
}
