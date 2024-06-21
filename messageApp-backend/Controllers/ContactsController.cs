using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using messageApp_backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace messageApp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ContactContext _context;
        private readonly MessageContext _messagesContext;
        private readonly UserContext _userContext;

        public ContactsController(ContactContext context, MessageContext messageContext, UserContext userContext)
        {
            _context = context;
            _messagesContext = messageContext;
            _userContext = userContext;
        }

        public class GetContactDTO : Contact
        {
            public string ContactUserName { get; set; }
            public FileContentResult? ContactProfilePicture { get; set; }
            public string? LastMessageContent { get; set; }
            public DateTime? LastMessageDate { get; set; }
            public MessageStatusAttribute.MessageStatus? LastMessageStatus { get; set; }
            public int? LastMessageSenderId { get; set; }
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts(int userId)
        {
            var contacts = await _context.Contacts
                .Include(c => c.ContactUser)
                .Include(c => c.LastMessage)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(contacts);
        }

        // PUT: api/Contacts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutContact(int id, Contact contact)
        {
            if (id != contact.Id)
            {
                return BadRequest();
            }

            _context.Entry(contact).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        public class PostContactDTO
        {
            public int UserId { get; set; }
            public int ContactId { get; set; }
        }

        // POST: api/Contacts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Contact>> PostContact([FromBody] PostContactDTO contactDTO)
        {
            var mainUser = await _userContext.Users.FindAsync(contactDTO.UserId);
            var contactUser = await _userContext.Users.FindAsync(contactDTO.ContactId);
            if (mainUser == null || contactUser == null)
            {
                return NotFound();
            }

            var newContact = new Contact
            {
                UserId = contactDTO.UserId,
                ContactUserId = contactDTO.ContactId,
                ContactUser = contactUser,
                LastMessageId = null
            };

            _context.Contacts.Add(newContact);
            await _context.SaveChangesAsync(); 
            await _context.Entry(newContact).Reference(c => c.ContactUser).LoadAsync();

            return Ok(newContact);
        }

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
