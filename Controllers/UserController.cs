using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static readonly List<User> _users = new()
        {
            new User { Id = 1, Name = "João Silva", Email = "joao@example.com" },
            new User { Id = 2, Name = "Maria Costa", Email = "maria@example.com" }
        };

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll() => Ok(_users);

        [HttpGet("{id}")]
        public ActionResult<User> GetById(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public ActionResult<User> Create(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User user)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();
            existing.Name = user.Name;
            existing.Email = user.Email;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return NotFound();
            _users.Remove(existing);
            return NoContent();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
