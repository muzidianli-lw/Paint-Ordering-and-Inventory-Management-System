using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.Models;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PaintStoreDbContext _dbContext;

        public UsersController(PaintStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            return Ok(await _dbContext.Users.ToListAsync(cancellationToken));
        }

        [HttpGet("by-user-id/{userId:int}")]
        public async Task<IActionResult> GetUsersById(int userId, CancellationToken cancellationToken)
        {
            if(userId < 0)
            {
                return BadRequest("userId < 0");
            }

            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == userId, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        } 

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetUsersByName(string name, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("name is null or whitespace");
            }
            return Ok(await _dbContext.Users.Where(u=>u.Name == name).ToListAsync(cancellationToken));
        } 

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUsersByEmail(string email, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("email is null or whitespace");
            }
            return Ok(await _dbContext.Users.Where(u=>u.Email== email).ToListAsync(cancellationToken));            
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user, CancellationToken cancellationToken)
        {
            User userUsed = new User(user.Name, user.Email, user.Phone);
            _dbContext.Users.Add(userUsed);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(nameof(GetUsersById), new {userId=userUsed.Id}, userUsed);
        } 
    }
}
