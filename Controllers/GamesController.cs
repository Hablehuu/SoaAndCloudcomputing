using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRestAPI.Models;
using MyRestAPI.DTOs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text;

namespace MyRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GameContext _gamecontext;
        private readonly developerContext _devcontext;
        private readonly ReviewContext _revcontext;
        private readonly UserContext _userContext;

        public GamesController(GameContext context, developerContext devContext, ReviewContext revcontext, UserContext userContext)
        {
            _gamecontext = context;
            _devcontext = devContext;
            _revcontext = revcontext;
            _userContext = userContext;
        }


        // GET: api/Games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            if (_gamecontext.Games == null)
            {
                return NotFound();
            }
            var game = await _gamecontext.Games.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            var response = new
            {
                Game = game,
                Links = new List<LinkDto>
            {
            new LinkDto
            {
                Href = Url.Action("GetGame",new { id = game.Id }),
                Rel = "self",
            },
            new LinkDto
            {
                Href = Url.Action("GetReviews",new { id = game.Id }),
                Rel = "Reviews for the game",
            },
            new LinkDto
            {
                Href = Url.Action("GetDeveloper",new { id = game.DeveloperID }),
                Rel = "Developer of the game",
            }
            }
            };



            return Ok(response);
        }

        // PUT: api/Games/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGame(int id, Game game)
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

                var users = _userContext.Users;
                IQueryable<User> userquery = users;
                userquery = userquery.Where(g => g.Name == nameandpass[0]);


                if (!userquery.First().admin) { return BadRequest("the user is not an admin"); }
            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }

            if (id != game.Id)
            {
                return BadRequest();
            }

            if(!GameExists(id)) { return NotFound("game you are trying change does not exist"); }
            if (GameExists(game.Id)) { return BadRequest("The id you are trying to give to the game is already in use"); }
            if (!DeveloperExists(game.DeveloperID)) { return BadRequest("The developer does not exist"); }
            var revs = _revcontext.Reviews;
            IQueryable<Review> query = revs;
            foreach (Review review in query) 
            { 
                if(review.GameID ==  id) 
                { 
                    _revcontext.Entry(review).State = EntityState.Modified;
                
                }
            }
            _gamecontext.Entry(game).State = EntityState.Modified;

            try
            {
                await _gamecontext.SaveChangesAsync();
                await _revcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameExists(id))
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

        // POST: api/Games
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Game>> PostGame(Game game)
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

                var users = _userContext.Users;
                IQueryable<User> userquery = users;
                userquery = userquery.Where(g => g.Name == nameandpass[0]);


                if (!userquery.First().admin) { return BadRequest("the user is not an admin"); }
            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }




            if (_gamecontext.Games == null)
            {
                return Problem("Entity set 'GameContext.Games'  is null.");
            }
            if (!(bool)(_devcontext.Developers?.Any(e => e.DeveloperID == game.DeveloperID)))
            {
                return Problem("Developer id does not exist");
            }
            if(_gamecontext.Games.ToArray().Length != 0)
            {
                if (!(bool)(_gamecontext.Games?.Any(e => e.Id == game.Id)))
                {
                    return Problem("The game with that id already exists");
                }
            }
            

            _gamecontext.Games.Add(game);

            await _gamecontext.SaveChangesAsync();

            //var gameUrl = Url.Action("GetGame", new { id = game.Id }, Request.Scheme);

            // Create a response object with HATEOAS links
            
            var response = new
            {
                Game = game,
                Links = new List<LinkDto>
            {
            new LinkDto
            {
                Href = Url.Action("GetGame",new { id = game.Id }),
                Rel = "self",
            },
            new LinkDto
            {
                Href = Url.Action("GetReviews",new { id = game.Id }),
                Rel = "Reviews for the game",
            },
            new LinkDto
            {
                Href = Url.RouteUrl("GetDeveloper",new { id = game.DeveloperID }),
                Rel = "Developer of the game",
            }
            }
            };

            return CreatedAtAction("GetGame", new { id = game.Id }, response);

           
        }

        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            if (_gamecontext.Games == null)
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

                var users = _userContext.Users;
                IQueryable<User> userquery = users;
                userquery = userquery.Where(g => g.Name == nameandpass[0]);


                if (!userquery.First().admin) { return BadRequest("the user is not an admin"); }
            }
            else
            {
                // Authorization header is not present in the request
                return BadRequest("Authorization header is missing.");
            }



            var game = await _gamecontext.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _gamecontext.Games.Remove(game);
            await _gamecontext.SaveChangesAsync();

            return NoContent();
        }
        //api/Games
        [HttpGet(Name = "SearchGames")]
        public IActionResult SearchGames(
            [FromQuery] string? name,
            [FromQuery] int? releaseYear,
            [FromQuery] string? releaseYearOperator,
            [FromQuery] string? Developer)
        {
            // Get the DbSet for the Game entity
            var games = _gamecontext.Games;
            IQueryable<Game> query = games;

            // Apply filters based on query parameters
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(g => g.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (releaseYear.HasValue)
            {
                if (!string.IsNullOrEmpty(releaseYearOperator))
                {
                    switch (releaseYearOperator)
                    {
                        case "<":
                            query = query.Where(g => g.ReleaseYear < releaseYear);
                            break;
                        case ">":
                            query = query.Where(g => g.ReleaseYear > releaseYear);
                            break;
                        case "<=":
                            query = query.Where(g => g.ReleaseYear <= releaseYear);
                            break;
                        case ">=":
                            query = query.Where(g => g.ReleaseYear >= releaseYear);
                            break;
                        default:
                            return BadRequest("Invalid releaseYearOperator. Supported values are '<', '>', '<=', '>='.");
                    }
                }
                else
                {
                    query = query.Where(g => g.ReleaseYear == releaseYear);
                }
            }

            if (!string.IsNullOrEmpty(Developer))
            {
                try
                {
                    int devID = _devcontext.Developers.Find(Developer).DeveloperID;
                    query = query.Where(g => g.DeveloperID == devID);
                } catch (Exception ex)
                { return NoContent(); }
                
            }
            // Execute the query and retrieve the results
            var searchResults = query.ToList();

            var response = new List<LinkDto>();
            response.Add(new LinkDto
            {
                Href = Url.Action("SearchGames"),
                Rel = "search keyvalues: name, releaseyear, releaseYearOperator and Developer",
            });

            var result = new { searchResults,response };
            return Ok(result);
        }

        [HttpGet("{id}/Reviews")]
        public IActionResult GetReviews(int id)
        {
            var revs = _revcontext.Reviews;
            if(!GameExists(id)) { return NotFound("Game does not exist"); }
            IQueryable<Review> query = revs;
            query = query.Where(g => g.GameID == id);
            if(query.ToArray().Length == 0)
            {

            }
            return Ok(revs.ToList());
        }

        private bool DeveloperExists(int id)
        {
            return (_devcontext.Developers?.Any(e => e.DeveloperID == id)).GetValueOrDefault();
        }


        [HttpGet("{id}/Info")]
        public IActionResult GetGameDTO(int id)
        {
            //create a GameDTO based on the 'id' parameter
            GameDto myDTO = GetGameInfoById(id);

            if (myDTO == null)
            {
                return NotFound(); // Return a 404 response if the data is not found
            }

            return Ok(myDTO); // Return the DTO as JSON in the response
        }

        private GameDto GetGameInfoById(int id)
        {
            var game = _gamecontext.Games.Find(id);
            
            if (game == null)
            {
                return null;
            }

            GameDto gameDto = new();
            gameDto.ReleaseYear = game.ReleaseYear;
            gameDto.Title = game.Name;
            gameDto.Developer = _devcontext.Developers.Find(game.DeveloperID).Name;
            return gameDto;
        }

        private bool GameExists(int id)
        {
            return (_gamecontext.Games?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private LinkDto HATEOASForGames(Game game, int id, string action)
        {

            return null;
        }

        private bool UserVerification(string name, string pass)
        {
            var users = _userContext.Users;
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
