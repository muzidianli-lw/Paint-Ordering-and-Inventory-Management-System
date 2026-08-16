using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Services;
using PaintStore.API.Services.Results;
using PaintStore.Models;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PaintStoreDbContext _dbContext;
        private readonly UsersService _usersService;

        public UsersController(PaintStoreDbContext dbContext, UsersService usersService)
        {
            _dbContext = dbContext;
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationOffsetRequestDto request, 
                                                        CancellationToken cancellationToken)
        {
            ServiceResult<PaginationOffsetResponseDto<UserResponseDto>> response 
                = await _usersService.GetAllUsers(request, cancellationToken);
            
            return response.State switch
            {
                ServiceResultsEnum.Success => Ok(),
                ServiceResultsEnum.BadRequest => BadRequest(response.ErrorMsg),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            UserResponseDto? user = await _dbContext.Users.Where(u=>u.Id == id)
                                                            .Select(UserResponseDto.Projection)
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Put([FromRoute] int id, 
                                                [FromBody] UserUpdateRequestDto userDto, 
                                                CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            bool emailExist = await _dbContext.Users.AnyAsync(u=>u.Email == userDto.Email.Trim() 
                                                                && u.Id != user.Id, cancellationToken);
            if (emailExist)
            {
                return Conflict("input email is existed");
            }

            _dbContext.Entry(user)
                .Property(u=>u.RowVersion)
                .OriginalValue = userDto.RowVersion;

            user.Update(userDto.Name, userDto.Email, userDto.Phone);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                bool userExist = await _dbContext.Users.AnyAsync(u=>u.Id == user.Id, cancellationToken);
                if (userExist)
                {
                    return Conflict("old data changed");
                }
                return NotFound();
            }
            catch (DbUpdateException exception)
                when(exception.InnerException is SqlException sqlException 
                    && sqlException.Number == 2601)
            {
                return Conflict("input email is existed");
            }
            return Ok(UserResponseDto.FromEntity(user));
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                int affectedRows = await _dbContext.Users.Where(u=>u.Id == id)
                                                .ExecuteDeleteAsync(cancellationToken);
                if(affectedRows == 0)
                {
                    return NotFound();
                }
            }
            catch(SqlException sqlException)
                when(sqlException.Number == 547)
            {
                return Conflict("related data existed"); 
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateRequestDto userDto, 
                                                CancellationToken cancellationToken)
        {
            bool emailExist = await _dbContext.Users.AnyAsync(u=>u.Email == userDto.Email.Trim(), 
                                                                cancellationToken);
            if (emailExist)
            {
                return Conflict("input email is existed");
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

            return CreatedAtAction(nameof(Get), new {user.Id}, UserResponseDto.FromEntity(user));
        }
    }
}
