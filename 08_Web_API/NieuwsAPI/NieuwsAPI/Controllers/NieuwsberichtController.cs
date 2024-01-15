using Microsoft.AspNetCore.Mvc;
using NieuwsAPI.Data;
using NieuwsAPI.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NieuwsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NieuwsberichtController : ControllerBase
    {
        NieuwsberichtRepository repository;
        public NieuwsberichtController(NieuwsberichtRepository repository)
        {
            this.repository = repository;
        }
        // GET: api/<NieuwsberichtController>
        [HttpGet]
        public IEnumerable<Nieuwsbericht> Get()
        {
            return repository.Messages;
        }

        // GET api/<NieuwsberichtController>/5
        [HttpGet("{id}")]
        public ActionResult<Nieuwsbericht> Get(int id)
        {
            if (repository.IdExists(id))
            {
                return repository[id];
            } else
            {
                return NotFound();
            }

        }

        // POST api/<NieuwsberichtController>
        [HttpPost]
        public ActionResult<Nieuwsbericht> Post(Nieuwsbericht bericht)
        {
            Nieuwsbericht nieuwbericht = repository.Add(bericht);
            return CreatedAtAction("Get", new { id = nieuwbericht.Id }, nieuwbericht);
        }

        // PUT api/<NieuwsberichtController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, Nieuwsbericht nieuwsbericht)
        {
            if (nieuwsbericht.Id == id)
            {
                repository.Update(nieuwsbericht);
                return NoContent();
            }

            else
            {
                // Use methods of ControllerBase to return status to client
                return BadRequest();
            }
        }

        // DELETE api/<NieuwsberichtController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.IdExists(id))
            {
                repository.Delete(id);
                // Use methods of ControllerBase to return status to client
                return NoContent();
            }
            else
            {
                // Use methods of ControllerBase to return status to client
                return NotFound();
            }

        }
    }
}
