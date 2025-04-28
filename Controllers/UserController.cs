using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.Models;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ? Get All Users with optional filtering and pagination
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDisplayDto>>> GetUsers(
            [FromQuery] string? name,
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.FullName!.Contains(name));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDisplayDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // ? Get Specific User
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDisplayDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("???????? ??? ?????");

            return Ok(new UserDisplayDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        // ? Create User (Admin only)
        [HttpPost]
        public async Task<ActionResult<UserDisplayDto>> CreateUser([FromBody] UserCreateDto dto)
        {
            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                Role = dto.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, new UserDisplayDto
            {
                Id = newUser.Id,
                FullName = newUser.FullName,
                Email = newUser.Email,
                Role = newUser.Role
            });
        }

        // ? Update User
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("???????? ??? ?????");

            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.Role = dto.Role ?? user.Role;

            await _context.SaveChangesAsync();
            return Ok("?? ????? ?????? ???????? ????? ?");
        }

        // ? Delete User
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("The user has been deleted successfully ?");
        }
    }

}
