using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using messageApp_backend.Models;

namespace messageApp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestesController : ControllerBase
    {
        private readonly TesteContext _context;

        public TestesController(TesteContext context)
        {
            _context = context;
        }

        // GET: api/Testes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teste>>> GetTestes()
        {
            return await _context.Testes.Include(t => t.User).ToListAsync();
        }

        // GET: api/Testes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Teste>> GetTeste(int id)
        {
            var teste = await _context.Testes.FindAsync(id);

            if (teste == null)
            {
                return NotFound();
            }

            return teste;
        }

        // PUT: api/Testes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeste(int id, Teste teste)
        {
            if (id != teste.Id)
            {
                return BadRequest();
            }

            _context.Entry(teste).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TesteExists(id))
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

        public class TesteDto
        {
            public string TesteName { get; set; }
            public int UserId { get; set; }
        }

        // POST: api/Testes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Teste>> PostTeste(TesteDto testeDto)
        {
            Teste teste = new Teste
            {
                TesteName = testeDto.TesteName,
                UserId = testeDto.UserId,
            };
            _context.Testes.Add(teste);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeste", new { id = teste.Id }, teste);
        }

        // DELETE: api/Testes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeste(int id)
        {
            var teste = await _context.Testes.FindAsync(id);
            if (teste == null)
            {
                return NotFound();
            }

            _context.Testes.Remove(teste);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TesteExists(int id)
        {
            return _context.Testes.Any(e => e.Id == id);
        }
    }
}
