using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ProductsAPI.Models; // Acede aos modelos User e Sale

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // Simplificado para evitar inicialização circular
        private static readonly List<User> _users = new()
        {
            new User { Id = 1, Username = "joaos", Email = "joao@example.com", FullName = "João Silva", Role = "Admin" },
            new User { Id = 2, Username = "mariac", Email = "maria@example.com", FullName = "Maria Costa", Role = "User" }
        };

        // ... (GetUsers, GetUser, CreateUser, DeleteUser mantidos)

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers([FromQuery] string role = null)
        {
            var users = _users.AsEnumerable();
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role.Equals(role, System.StringComparison.OrdinalIgnoreCase));
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public ActionResult<User> CreateUser(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            user.Sales ??= new List<Sale>(); 
            _users.Add(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, User updatedUser)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();

            existing.Username = updatedUser.Username;
            existing.Email = updatedUser.Email;
            existing.FullName = updatedUser.FullName;
            existing.Role = updatedUser.Role;
            // Se as Sales forem enviadas no payload, atualiza. Caso contrário, mantém.
            if(updatedUser.Sales != null) existing.Sales = updatedUser.Sales;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();
            _users.Remove(existing);
            return NoContent();
        }
    }
}