using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private static readonly List<Supplier> _suppliers = new()
        {
            new Supplier { Id = 1, Name = "Tech Components Ltda", Email = "contato@techcomp.com" },
            new Supplier { Id = 2, Name = "GlobalParts S.A.", Email = "vendas@globalparts.com" }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Supplier>> GetAll() => Ok(_suppliers);

        [HttpGet("{id}")]
        public ActionResult<Supplier> GetById(int id)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            return supplier == null ? NotFound() : Ok(supplier);
        }

        [HttpPost]
        public ActionResult<Supplier> Create(Supplier supplier)
        {
            supplier.Id = _suppliers.Any() ? _suppliers.Max(s => s.Id) + 1 : 1;
            _suppliers.Add(supplier);
            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Supplier supplier)
        {
            var existing = _suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            existing.Name = supplier.Name;
            existing.Email = supplier.Email;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            _suppliers.Remove(existing);
            return NoContent();
        }
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
