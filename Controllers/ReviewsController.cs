using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRestAPI.Models;

namespace MyRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewContext _context;
        private readonly GameContext _gamecontext;
        private readonly UserContext _usercontext;

        public ReviewsController(ReviewContext context, GameContext gamecontext, UserContext usercontext)
        {
            _context = context;
            _gamecontext = gamecontext;
            _usercontext = usercontext;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
          if (_context.Reviews == null)
          {
              return NotFound();
          }
            return await _context.Reviews.ToListAsync();
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
          if (_context.Reviews == null)
          {
              return NotFound();
          }
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // PUT: api/Reviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, Review review)
        {
            if (id != review.Id)
            {
                return BadRequest();
            }
            string[] nameandpass;
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
                nameandpass = usercredentials.Split(':');

                if (!UserVerification(nameandpass[0], nameandpass[1]))
                {
                    return BadRequest("name or password do not match");
                }

            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }


            var oldreview = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            if (oldreview.Author != nameandpass[0] || review.Author != nameandpass[0]) { return BadRequest("names don't match"); }
            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
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

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
          if (_context.Reviews == null)
          {
              return Problem("Entity set 'ReviewContext.Reviews'  is null.");
          }
            string[] nameandpass;
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
                nameandpass = usercredentials.Split(':');

                if (!UserVerification(nameandpass[0], nameandpass[1]))
                {
                    return BadRequest("name or password do not match");
                }

            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }

            if (review.Author != nameandpass[0]) { return BadRequest("name and author name do not match"); } 

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReview", new { id = review.Id }, review);
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (_context.Reviews == null)
            {
                return NotFound();
            }

            string[] nameandpass;
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
                nameandpass = usercredentials.Split(':');

                if (!UserVerification(nameandpass[0], nameandpass[1]))
                {
                    return BadRequest("name or password do not match");
                }

            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            if(review.Author != nameandpass[0]) { return BadRequest("you don't have permisson to delete this review"); }
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
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



        private bool ReviewExists(int id)
        {
            return (_context.Reviews?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
