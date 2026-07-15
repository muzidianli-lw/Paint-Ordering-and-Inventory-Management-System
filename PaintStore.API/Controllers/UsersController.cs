using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Data;
using PaintStore.Models;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetUsers()
        {
            return Ok(MockDataUsers.Users.ToList());
        }

        [HttpGet("by-user-id/{userId:int}")]
        public ActionResult GetUsersById(int userId)
        {
            if(userId < 0)
            {
                return BadRequest("userId < 0");
            }

            User? user = MockDataUsers.Users.FirstOrDefault(u=>u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        } 

        [HttpGet("by-name/{name:string}")]
        public ActionResult GetUsersByName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("name is null or whitespace");
            }
            return Ok(MockDataUsers.Users.Where(u=>u.Name == name).ToList());
        } 

        [HttpGet("by-email/{email:string}")]
        public ActionResult GetUsersByEmail(string email)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("email is null or whitespace");
            }
            return Ok(MockDataUsers.Users.Where(u=>u.Email== email).ToList());            
        } 
    }
}
