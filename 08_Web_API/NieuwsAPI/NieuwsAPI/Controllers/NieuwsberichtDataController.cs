using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NieuwsAPI.Data;
using NieuwsAPI.Model;

namespace NieuwsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Nu in Program.cs, kan ook hier
    // [EnableCors("AllowAllOrgin")]
    public class NieuwsberichtDataController : ControllerBase
    {
        private readonly NieuwsAPIContext _context;

        public NieuwsberichtDataController(NieuwsAPIContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Toont alle nieuwsberichten
        /// </summary>
        /// <returns>
        /// Een collectie van nieuwsberichten
        /// </returns>

        // GET: api/NieuwsberichtData
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nieuwsbericht>>> GetNieuwsbericht()
        {
            return await _context.Nieuwsbericht.ToListAsync();
        }

        /// <summary>
        /// Haalt een specifiek nieuwsbericht op
        /// </summary>
        /// <param name="id"></param> 
        /// 
        // GET: api/NieuwsberichtData/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Nieuwsbericht>> GetNieuwsbericht(int? id)
        {
            var nieuwsbericht = await _context.Nieuwsbericht.FindAsync(id);

            if (nieuwsbericht == null)
            {
                return NotFound();
            }

            return nieuwsbericht;
        }


        /// <summary>
        /// Past een specifiek nieuwsbericht aan
        /// </summary>
        /// <param name="id">unieke id van het nieuwsbericht</param>   
        /// <param name="nieuwsbericht">aangepaste nieuwsbericht</param>  

        // PUT: api/NieuwsberichtData/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNieuwsbericht(int? id, Nieuwsbericht nieuwsbericht)
        {
            if (id != nieuwsbericht.Id)
            {
                return BadRequest();
            }

            _context.Entry(nieuwsbericht).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NieuwsberichtExists(id))
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

        /// <summary>
        /// Voeg een nieuw nieuwsbericht toe
        /// </summary>
        /// <remarks>
        /// Voorbeeld request:
        /// 
        ///     POST api/NewsMessages
        ///     {
        ///         "titel": "tweede bericht",
        ///         "bericht": "inhoud bericht",
        ///         "datum": "2020-11-13T11:37:59.833Z"
        ///     }
        ///     
        /// </remarks>
        /// <param name="nieuwsbericht">nieuw nieuwsbericht</param>   


        // POST: api/NieuwsberichtData
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Nieuwsbericht>> PostNieuwsbericht(Nieuwsbericht nieuwsbericht)
        {
            _context.Nieuwsbericht.Add(nieuwsbericht);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNieuwsbericht", new { id = nieuwsbericht.Id }, nieuwsbericht);
        }

        /// <summary>
        /// Verwijdert een specifiek nieuwsbericht
        /// </summary>
        /// <param name="id"></param>   
        /// <response code="204">Empty response indicating a successfull delete</response>
        /// <response code="404">The item to delete is not found</response>    


        // DELETE: api/NieuwsberichtData/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNieuwsbericht(int? id)
        {
            var nieuwsbericht = await _context.Nieuwsbericht.FindAsync(id);
            if (nieuwsbericht == null)
            {
                return NotFound();
            }

            _context.Nieuwsbericht.Remove(nieuwsbericht);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NieuwsberichtExists(int? id)
        {
            return _context.Nieuwsbericht.Any(e => e.Id == id);
        }
    }
}
