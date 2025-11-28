using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ProductsAPI.Models;
using ProductsAPI.Data;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers([FromQuery] string? role = null)
        {
            var users = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role);
            }
            return Ok(users.ToList());
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            return user == null ? NotFound() : Ok(user);
        }

        // --- MUDANÇA AQUI ---
        // Usamos a classe "UserCreateRequest" (definida lá em baixo) em vez de "User".
        // Assim o Swagger mostra apenas os campos que queremos (sem ID).
        [HttpPost]
        public ActionResult<User> CreateUser(UserCreateRequest request)
        {
            // Converter o pedido (Request) para o Modelo Real (User)
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role
                // O ID não é definido, o SQL Server gera-o sozinho.
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, User updatedUser)
        {
            var existing = _context.Users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();

            existing.Username = updatedUser.Username;
            existing.Email = updatedUser.Email;
            existing.FullName = updatedUser.FullName;
            existing.Role = updatedUser.Role;
            
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var existing = _context.Users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();

            _context.Users.Remove(existing);
            _context.SaveChanges();
            return NoContent();
        }
    }

    // ==========================================================
    // CLASSE AUXILIAR (Está no mesmo ficheiro para não criar novos)
    // ==========================================================
    public class UserCreateRequest
    {
        // Nota: Não colocamos o ID aqui, por isso o Swagger não o pede.
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}