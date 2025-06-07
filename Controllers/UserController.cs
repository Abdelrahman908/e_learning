using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly PasswordHasher<User> _passwordHasher;

        public UserController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // ✅ Get All Users with optional filtering and pagination
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

        // ✅ Get Specific User
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDisplayDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("المستخدم غير موجود.");

            return Ok(new UserDisplayDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        // ✅ Create User (Admin only)
        [HttpPost]
        public async Task<ActionResult<UserDisplayDto>> CreateUser([FromBody] UserCreateDto dto)
        {
            // تحقق من البريد الإلكتروني
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("البريد الإلكتروني مستخدم مسبقًا.");

            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role
            };

            // تشفير كلمة المرور
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.Password);

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

        // ✅ Update User
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("المستخدم غير موجود.");

            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.Role = dto.Role ?? user.Role;

            await _context.SaveChangesAsync();
            return Ok("تم تحديث المستخدم بنجاح.");
        }

        // ✅ Delete User
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("المستخدم غير موجود.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("تم حذف المستخدم بنجاح.");
        }
    }
}
