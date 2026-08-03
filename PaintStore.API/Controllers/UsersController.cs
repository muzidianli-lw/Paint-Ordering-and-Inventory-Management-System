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
            List<UserResponseDto> usersDto = [];
            foreach(var user in users)
            {
                usersDto.Add(new UserResponseDto(){Name=user.Name, Email=user.Email, Phone=user.Phone});
            }
            return Ok(usersDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            if(id < 0)
            {
                return BadRequest("id < 0");
            }

            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserResponseDto(){Name=user.Name, Email=user.Email, Phone=user.Phone});
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UserUpdateRequestDto userDto, CancellationToken cancellationToken)
        {
            if(id < 1)
            {
                return BadRequest("input id error");
            }

            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
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
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Email == userDto.Email, cancellationToken);
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
