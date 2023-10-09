using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRestAPI.Models;

namespace MyRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopersController : ControllerBase
    {
        private readonly developerContext _context;
        private readonly GameContext _gamecontext;
        private readonly UserContext _usercontext;

        public DevelopersController(GameContext gamecontext, developerContext Context, UserContext userContext)
        {
            _context = Context;
            _gamecontext = gamecontext;
            _usercontext = userContext;
        }

        // GET: api/Developers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Developer>>> GetDevelopers()
        {
          if (_context.Developers == null)
          {
              return NotFound();
          }
            return await _context.Developers.ToListAsync();
        }

        // GET: api/Developers/5
        [HttpGet("{id}", Name = "GetDeveloper")]
        public async Task<ActionResult<Developer>> GetDeveloper(int id)
        {
          if (_context.Developers == null)
          {
              return NotFound();
          }
            var developer = await _context.Developers.FindAsync(id);

            if (developer == null)
            {
                return NotFound();
            }

            return developer;
        }

        // PUT: api/Developers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeveloper(int id, Developer developer)
        {
            if (HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                string authorizationHeaderValue = HttpContext.Request.Headers["Authorization"];
                if (!authorizationHeaderValue.StartsWith("Basic"))
                {
                    BadRequest("API only supports basic Authorization");
                }
                string[] details = authorizationHeaderValue.Split(' ');
                // Convert Base64 string to bytes
                byte[] bytes = Convert.FromBase64String(details[1]);

                // Convert bytes to a string using a specific encoding (e.g., UTF-8)
                string usercredentials = Encoding.UTF8.GetString(bytes);
                string[] nameandpass = usercredentials.Split(':');

                if (!UserVerification(nameandpass[0], nameandpass[1]))
                {
                    return BadRequest("name or password do not match");
                }

                var users = _usercontext.Users;
                IQueryable<User> userquery = users;
                userquery = userquery.Where(g => g.Name == nameandpass[0]);


                if (!userquery.First().admin) { return BadRequest("the user is not an admin"); }
            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }



            if (id != developer.DeveloperID)
            {
                return BadRequest();
            }

            


            _context.Entry(developer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeveloperExists(id))
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

        // POST: api/Developers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Developer>> PostDeveloper(Developer developer)
        {
          if (_context.Developers == null)
          {
              return Problem("Entity set 'developerContext.Developers'  is null.");
          }

            if (HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                string authorizationHeaderValue = HttpContext.Request.Headers["Authorization"];
                if (!authorizationHeaderValue.StartsWith("Basic"))
                {
                    BadRequest("API only supports basic Authorization");
                }
                string[] details = authorizationHeaderValue.Split(' ');
                // Convert Base64 string to bytes
                byte[] bytes = Convert.FromBase64String(details[1]);

                // Convert bytes to a string using a specific encoding (e.g., UTF-8)
                string usercredentials = Encoding.UTF8.GetString(bytes);
                string[] nameandpass = usercredentials.Split(':');

                if (!UserVerification(nameandpass[0], nameandpass[1]))
                {
                    return BadRequest("name or password do not match");
                }

                var users = _usercontext.Users;
                IQueryable<User> userquery = users;
                userquery = userquery.Where(g => g.Name == nameandpass[0]);


                if (!userquery.First().admin) { return BadRequest("the user is not an admin"); }
            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }


            var rand = new Random();
            developer.DeveloperID = rand.Next();
            _context.Developers.Add(developer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDeveloper", new { id = developer.DeveloperID }, developer);
        }

        // DELETE: api/Developers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeveloper(int id, string name, string pass)
        {
            if (_context.Developers == null)
            {
                return NotFound();
            }

            if (HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                string authorizationHeaderValue = HttpContext.Request.Headers["Authorization"];
                if (!authorizationHeaderValue.StartsWith("Basic"))
                {
                    BadRequest("API only supports basic Authorization");
                }
                string[] details = authorizationHeaderValue.Split(' ');
                // Convert Base64 string to bytes
                byte[] bytes = Convert.FromBase64String(details[1]);

                // Convert bytes to a string using a specific encoding (e.g., UTF-8)
                string usercredentials = Encoding.UTF8.GetString(bytes);
                string[] nameandpass = usercredentials.Split(':');

                if (!UserVerification(nameandpass[0], nameandpass[1]))
                {
                    return BadRequest("name or password do not match");
                }

                var users = _usercontext.Users;
                IQueryable<User> userquery = users;
                userquery = userquery.Where(g => g.Name == nameandpass[0]);


                if (!userquery.First().admin) { return BadRequest("the user is not an admin"); }
            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }

            var developer = await _context.Developers.FindAsync(id);
            if (developer == null)
            {
                return NotFound();
            }

            _context.Developers.Remove(developer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeveloperExists(int id)
        {
            return (_context.Developers?.Any(e => e.DeveloperID == id)).GetValueOrDefault();
        }

        [HttpGet("{id}/Games")]
        public IActionResult GetAllGames(int id)
        {
            
            var games = _gamecontext.Games;
            IQueryable<Game> query = games;
            query = query.Where(g => g.DeveloperID == id);
            return Ok(query.ToList());
        }

        private bool UserVerification(string name, string pass)
        {

            var users = _usercontext.Users;
            IQueryable<User> query = users;
            query = query.Where(g => g.Name == name);
            if (!(query.ToArray().Length == 0))
            {
                if (query.ToArray()[0].Password == pass)
                {
                    return true;
                }
            }
            return false;
        }


    }
}
