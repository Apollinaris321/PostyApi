using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorksheetController : ControllerBase
    {
        private readonly TodoContext _context;

        public WorksheetController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/Worksheet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Worksheet>>> GetWorksheets()
        {
          if (_context.Worksheets == null)
          {
              return NotFound();
          }
            return await _context.Worksheets.ToListAsync();
        }

        // GET: api/Worksheet/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Worksheet>> GetWorksheet(long id)
        {
          if (_context.Worksheets == null)
          {
              return NotFound();
          }
            var worksheet = await _context.Worksheets.FindAsync(id);

            if (worksheet == null)
            {
                return NotFound();
            }

            return worksheet;
        }

        // PUT: api/Worksheet/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorksheet(long id, Worksheet worksheet)
        {
            if (id != worksheet.Id)
            {
                return BadRequest();
            }

            _context.Entry(worksheet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorksheetExists(id))
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

        // POST: api/Worksheet
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("/")]
        public async Task<ActionResult<Worksheet>> PostWorksheet(Worksheet worksheet)
        {
          if (_context.Worksheets == null)
          {
              return Problem("Entity set 'TodoContext.Worksheets'  is null.");
          }

          if (worksheet.ProfileId != null)
          {
              var owner = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == worksheet.ProfileId);
              Console.WriteLine($"hello i found user ; {owner.Worksheets.Count}");
              owner?.Worksheets.Add(worksheet);
          }
          _context.Worksheets.Add(worksheet);
          await _context.SaveChangesAsync();

          return CreatedAtAction("GetWorksheet", new { id = worksheet.Id }, worksheet);
        }

        // DELETE: api/Worksheet/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorksheet(long id)
        {
            if (_context.Worksheets == null)
            {
                return NotFound();
            }
            var worksheet = await _context.Worksheets.FindAsync(id);
            if (worksheet == null)
            {
                return NotFound();
            }

            _context.Worksheets.Remove(worksheet);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WorksheetExists(long id)
        {
            return (_context.Worksheets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
