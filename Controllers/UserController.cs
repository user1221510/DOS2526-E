using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // Alterado para 'UsersController' para seguir a convenção de nomeação do .NET Core
        [cite_start]// e consistência com o requisito do PDF (UsersController)[cite: 34].

        // Atualizado para incluir os novos campos no mock data, conforme o novo modelo User.
        private static readonly List<User> _users = new()
        {
            new User { Id = 1, Username = "joaos", Email = "joao@example.com", FullName = "João Silva", Role = "Admin" },
            new User { Id = 2, Username = "mariac", Email = "maria@example.com", FullName = "Maria Costa", Role = "User" }
        };

        [cite_start]// 1. GetUsers() com query parameters
        // Substitui o GetAll() e adiciona a lógica para filtrar com query parameters (ex: por Role)
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers([FromQuery] string role = null)
        {
            [cite_start]// O GetAll() existente é renomeado para GetUsers()
            var users = _users.AsEnumerable();

            if (!string.IsNullOrEmpty(role))
            {
                // Exemplo de uso de query parameter: filtrar por Role
                users = users.Where(u => u.Role.Equals(role, System.StringComparison.OrdinalIgnoreCase));
            }

            return Ok(users);
        }

        [cite_start]// 2. GetUser(int id)
        [cite_start]// O GetById(int id) existente é renomeado para GetUser(int id)
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return user == null ? NotFound() : Ok(user);
        }

        [cite_start]// 3. CreateUser(User user)
        [cite_start]// O Create(User user) existente é renomeado para CreateUser(User user)
        [HttpPost]
        public ActionResult<User> CreateUser(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            [cite_start]// Uso de nameof(GetUser) já que renomeamos o método GetById
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [cite_start]// 4. UpdateUser(User updatedUser)
        [cite_start]// O Update(int id, User user) existente é modificado para UpdateUser(User updatedUser)
        // Foi mantido o {id} no atributo [HttpPut] para um Update RESTful adequado.
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();

            // Atualizando todos os campos do modelo (incluindo os novos)
            existing.Username = updatedUser.Username; // Novo campo [cite: 36]
            existing.Email = updatedUser.Email;
            existing.FullName = updatedUser.FullName; // Novo campo [cite: 36]
            existing.Role = updatedUser.Role; // Novo campo [cite: 36]

            return NoContent();
        }

        [cite_start]// 5. DeleteUser(int id)
        [cite_start]// O Delete(int id) existente é renomeado para DeleteUser(int id)
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();
            _users.Remove(existing);
            return NoContent();
        }
    }

    [cite_start]// Modelo de Dados 'User' Atualizado
    public class User
    {
        public int Id { get; set; } // [cite: 36]
        public string Username { get; set; } // [cite: 36]
        public string Email { get; set; } // [cite: 36]
        public string FullName { get; set; } // [cite: 36]
        public string Role { get; set; } // [cite: 36]
    }
}