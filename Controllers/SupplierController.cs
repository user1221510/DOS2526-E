using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ProductsAPI.Models; // Usar namespace dos modelos

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        // Mock data de Suppliers, inicializando a lista de Products
        private static readonly List<Supplier> _suppliers = new()
        {
            new Supplier { Id = 1, Name = "Tech Components Ltda", Email = "contato@techcomp.com", Products = new List<Product>() },
            new Supplier { Id = 2, Name = "GlobalParts S.A.", Email = "vendas@globalparts.com", Products = new List<Product>() }
        };

        // 1. GetSuppliers() (Antigo GetAll())
        [HttpGet]
        public ActionResult<IEnumerable<Supplier>> GetSuppliers() => Ok(_suppliers);

        // 2. GetSupplier(int id) (Antigo GetById(int id))
        [HttpGet("{id}")]
        public ActionResult<Supplier> GetSupplier(int id)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            return supplier == null ? NotFound() : Ok(supplier);
        }

        // 3. CreateSupplier(Supplier supplier) (Antigo Add(Supplier supplier))
        [HttpPost]
        public ActionResult<Supplier> CreateSupplier(Supplier supplier)
        {
            supplier.Id = _suppliers.Any() ? _suppliers.Max(s => s.Id) + 1 : 1;
            supplier.Products ??= new List<Product>();
            _suppliers.Add(supplier);
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
        }

        // 4. UpdateSupplier(Supplier updatedSupplier)
        [HttpPut("{id}")]
        public IActionResult UpdateSupplier(int id, Supplier updatedSupplier)
        {
            var existing = _suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            existing.Name = updatedSupplier.Name;
            existing.Email = updatedSupplier.Email;
            // Atualiza a lista de Products (se fornecida no payload)
            if(updatedSupplier.Products != null) existing.Products = updatedSupplier.Products; 
            
            return NoContent(); // Retorna 204 NoContent
        }

        // 5. DeleteSupplier(int id)
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