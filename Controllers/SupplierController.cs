using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ProductsAPI.Models; // Acede aos modelos Supplier e Product

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        // O modelo Supplier deve ter sido movido para ProductsAPI.Models.
        private static readonly List<Supplier> _suppliers = new()
        {
            // Inicialização simples
            new Supplier { Id = 1, Name = "Tech Components Ltda", Email = "contato@techcomp.com" },
            new Supplier { Id = 2, Name = "GlobalParts S.A.", Email = "vendas@globalparts.com" }
        };

        // ... (GetSuppliers, GetSupplier, CreateSupplier, DeleteSupplier mantidos)

        [HttpGet]
        public ActionResult<IEnumerable<Supplier>> GetSuppliers() => Ok(_suppliers);

        [HttpGet("{id}")]
        public ActionResult<Supplier> GetSupplier(int id)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            return supplier == null ? NotFound() : Ok(supplier);
        }

        [HttpPost]
        public ActionResult<Supplier> CreateSupplier(Supplier supplier)
        {
            supplier.Id = _suppliers.Any() ? _suppliers.Max(s => s.Id) + 1 : 1;
            supplier.Products ??= new List<Product>();
            _suppliers.Add(supplier);
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSupplier(int id, Supplier updatedSupplier)
        {
            var existing = _suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            existing.Name = updatedSupplier.Name;
            existing.Email = updatedSupplier.Email;
            if(updatedSupplier.Products != null) existing.Products = updatedSupplier.Products; 
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSupplier(int id)
        {
            var existing = _suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            _suppliers.Remove(existing);
            return NoContent(); 
        }
    }
}