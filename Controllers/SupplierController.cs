using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Data;
using ProductsAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Adicionado o construtor para receber a Base de Dados
        public SupplierController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Supplier>> GetSuppliers()
        {
            return Ok(_context.Suppliers.ToList());
        }

        [HttpGet("{id}")]
        public ActionResult<Supplier> GetSupplier(int id)
        {
            var supplier = _context.Suppliers.FirstOrDefault(s => s.Id == id);
            return supplier == null ? NotFound() : Ok(supplier);
        }

        [HttpPost]
        public ActionResult<Supplier> CreateSupplier(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSupplier(int id, Supplier updatedSupplier)
        {
            var existing = _context.Suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            existing.Name = updatedSupplier.Name;
            existing.Email = updatedSupplier.Email;
            
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSupplier(int id)
        {
            var existing = _context.Suppliers.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            _context.Suppliers.Remove(existing);
            _context.SaveChanges();
            return NoContent();
        }
    }
}