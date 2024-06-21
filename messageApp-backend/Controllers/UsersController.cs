using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using messageApp_backend.Models;
using messageApp_backend.models;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.WebEncoders.Testing;
using static messageApp_backend.Controllers.UsersController;

namespace messageApp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly MessageContext _messageContext;
        private readonly IConfiguration _configuration;

        public UsersController(UserContext context, MessageContext messageContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            _messageContext = messageContext;

            _context.Database.EnsureCreated();
        }

        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.Where(u => u.IsActive).ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        public class LoginResponseDto
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Token { get; set; }
            public FileContentResult? ProfilePicture { get; set; }
        } 

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(UserDto request)
        {
            try
            {
                var user = await _context.Users
                .Where(u => u.IsActive && u.UserName == request.UserName)
                .SingleOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.password, user.PasswordHash))
                {
                    return BadRequest("Invalid password");
                }

                var token = CreateToken(user);
                FileContentResult? ProfilePicture = null;

                if (user.ProfilePicture != null)
                {
                    ProfilePicture = File(user.ProfilePicture, "image/jpeg");
                }

                return Ok(new LoginResponseDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Token = token,
                    ProfilePicture = ProfilePicture
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(int id, [FromForm] User userDto, [FromForm] IFormFile? file)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            if (file != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    user.ProfilePicture = memoryStream.ToArray();
                }
            }
            user.UserName = userDto.UserName;
            _context.Update(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(user.ProfilePicture != null ? File(user.ProfilePicture, "image/jpeg") : null);
        }


        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserDto userDto)
        {
            if (_context.Users.Any(u => u.IsActive && u.UserName == userDto.UserName))
            {
                return BadRequest("UserName already registered");
            }

            var PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.password);

            var newUser = new User
            {
                UserName = userDto.UserName,
                PasswordHash = PasswordHash,
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostUser), new { id = newUser.Id }, newUser);
        }

        public class PostContactDto
        {
            public int userId { get; set; }
            public int contactId { get; set; }
        }

        [HttpPost("/api/Users/addContato")]
        [Authorize]
        public async Task<ActionResult<User>> PostUserContact(PostContactDto postContactDto)
        {
            var user = await _context.Users
                             .Include(u => u.Contacts)
                             .FirstOrDefaultAsync(u => u.Id == postContactDto.userId);
            var contact = await _context.Users.FindAsync(postContactDto.contactId);

            if (contact == null)
            {
                return NotFound();
            }

            if (user.Contacts.Contains(contact))
            {
                return BadRequest("Contato já existe.");
            }

            user.Contacts.Add(contact);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpGet("/api/Users/{userId}/Contatos")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserContacts(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Contacts)
                                 .FirstOrDefaultAsync(u => u.Id == userId);

            if (user.Contacts == null)
            {
                return Ok(Array.Empty<User>());
            }

            var response = user.Contacts.Select(c =>
            {
                var lastMessage = _messageContext.Messages
                    .Where(m => (m.UsuarioDestinatarioId == c.Id && m.UsuarioRemetenteId == userId) ||
                                (m.UsuarioDestinatarioId == userId && m.UsuarioRemetenteId == c.Id))
                    .OrderByDescending(m => m.DataEnvio)
                    .FirstOrDefault();

                var ProfilePicture = c.ProfilePicture != null ? File(c.ProfilePicture, "image/jpeg") : null;

                return new
                {
                    c.Id,
                    c.UserName,
                    ProfilePicture,
                    lastMessage,
                };
            });

            return Ok(response);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Where(u => u.IsActive).Any(e => e.Id == id);
        }
    }
}
