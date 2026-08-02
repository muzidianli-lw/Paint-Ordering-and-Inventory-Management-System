using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
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
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
        {
            List<User> users = await _dbContext.Users.ToListAsync(cancellationToken);
            if (users.Count == 0)
            {
                return Ok();
            }
            List<UserResponseDto> usersDto = [];
            foreach(var user in users)
            {
                usersDto.Add(new UserResponseDto(){Name=user.Name, Email=user.Email, Phone=user.Phone});
            }
            return Ok(usersDto);
        }

        [HttpGet("{Id:int}")]
        public async Task<IActionResult> Get(int Id, CancellationToken cancellationToken)
        {
            if(Id < 0)
            {
                return BadRequest("Id < 0");
            }

            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == Id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost("{Id:int}")]
        public async Task<IActionResult> Put([FromRoute] int Id, [FromBody] UserCreateRequestDto userDto, CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == Id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.Phone = userDto.Phone;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            UserResponseDto userResponseDto = new UserResponseDto(){Name=user.Name, Email=user.Email, Phone=user.Phone};
            return Ok(userResponseDto);
        }

        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> Delete(int Id, CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == Id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateRequestDto userDto, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(userDto.Name))
            {
                return BadRequest("input name error");
            }
            if(string.IsNullOrWhiteSpace(userDto.Email))
            {
                return BadRequest("input email error");
            }
            if(string.IsNullOrWhiteSpace(userDto.Phone))
            {
                return BadRequest("input phone number error");
            }

            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Email == userDto.Email);
            if (user != null)
            {
                return BadRequest("input Email duplicated");
            }

            user = new User(userDto.Name, userDto.Email, userDto.Phone);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(nameof(Get), new {user.Id}, user);
        }
    }
}
