using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            List<UserResponseDto> users = await _dbContext.Users
                                                    .Select(u => new UserResponseDto ()
                                                            {
                                                                Id = u.Id,
                                                                Name=u.Name, 
                                                                Email=u.Email, 
                                                                Phone=u.Phone
                                                            })
                                                    .ToListAsync(cancellationToken);
            return Ok(users);
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            UserResponseDto? user = await _dbContext.Users.Where(u=>u.Id == id)
                                                            .Select(u => new UserResponseDto ()
                                                                        {
                                                                            Id = u.Id,
                                                                            Name=u.Name, 
                                                                            Email=u.Email, 
                                                                            Phone=u.Phone
                                                                        })
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UserUpdateRequestDto userDto, CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            bool emailExist = await _dbContext.Users.AnyAsync(u=>u.Email == userDto.Email && u.Id != user.Id, cancellationToken);
            if (emailExist)
            {
                return BadRequest("input Email duplicated");
            }

            user.Update(userDto.Name, userDto.Email, userDto.Phone);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new UserResponseDto(){Id=user.Id, Name=user.Name, Email=user.Email, Phone=user.Phone});
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            _dbContext.Users.Remove(user);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch(DbUpdateConcurrencyException)
            {
                return NoContent();
            }
            catch(DbUpdateException exception)
                when( exception.InnerException is SqlException sqlException
                    && sqlException.Number == 547)
            {
                return Conflict("related data existed"); 
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateRequestDto userDto, CancellationToken cancellationToken)
        {
            bool emailExist = await _dbContext.Users.AnyAsync(u=>u.Email == userDto.Email, cancellationToken);
            if (emailExist)
            {
                return BadRequest("input Email duplicated");
            }

            User user = new User(userDto.Name, userDto.Email, userDto.Phone);
            _dbContext.Users.Add(user);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch(DbUpdateException exception)
                when(exception.InnerException is SqlException sqlException
                    && sqlException.Number == 2601)
            {
                return Conflict("input email is existed");
            }

            return CreatedAtAction(nameof(Get), new {user.Id}, new UserResponseDto()
                                                                {
                                                                    Id=user.Id, 
                                                                    Name=user.Name, 
                                                                    Email=user.Email, 
                                                                    Phone=user.Phone});
        }
    }
}
