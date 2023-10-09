using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRestAPI.Models;
using MyRestAPI.DTOs;
using System.Text;

namespace MyRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;


        public UsersController(UserContext context)
        {
            _context = context;

            if (!context.Users.Any())
            {
                // Create an initial admin user and add it to the database
                var adminUser = new User
                {
                    Name = "admin",
                    Id = 1,
                    Password = "admin",
                    admin = true

                };

                context.Users.Add(adminUser);
                context.SaveChanges();

            }

        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
          if (_context.Users == null)
          {
              return NotFound();
          }

            var users = new List<UserDto>();


            foreach (var user in _context.Users)
            {
                UserDto userDto = new();
                userDto.Name = user.Name;
                userDto.Id = user.Id;
                users.Add(userDto);

            }
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            UserDto userDto = new UserDto();
            userDto.Name = user.Name;
            userDto.Id= user.Id;
            return userDto;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(User user,int id)
        {
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

            var newuser = _context.Users.Find(id);
            if (newuser == null) { BadRequest("user not found"); }
            if((bool)(_context.Users?.Any(e => e.Name == user.Name)) && user.Name != nameandpass[0])
            {
                return BadRequest("username already exists");
            }
            newuser.Name = user.Name;
            newuser.Password = user.Password;
            _context.Entry(newuser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
          if (_context.Users == null)
          {
              return Problem("Entity set 'UserContext.Users'  is null.");
          }
          if(_context.Users.Any(e => e.Name == user.Name))
            {
                return BadRequest("user with that name already exists");
            }
            try
            {
                var rand = new Random();
                user.Id = rand.Next();
                user.admin = false;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                return BadRequest("the id is already in use");
            }
            

            return CreatedAtAction("GetUser", new { Id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string name, string pass,int id)
        {
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
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("user was not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserVerification(string name, string pass)
        {

            var users = _context.Users;
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
        private bool IsTheUser(string name, string pass,User olduser)
        {
            var user = _context.Users.Find(name);
            if (!(user == null))
            {
                if (user.Password == pass)
                {
                    return true;
                }
            }
            return false;

        }


        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
