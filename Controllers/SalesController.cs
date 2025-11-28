using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsAPI.Data;
using ProductsAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Sale>> GetSales()
        {
            var sales = _context.Sales
                .Include(s => s.User)
                .Include(s => s.Products)
                .ToList();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public ActionResult<Sale> GetSale(int id)
        {
            var sale = _context.Sales
                .Include(s => s.User)
                .Include(s => s.Products)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null) return NotFound();
            return Ok(sale);
        }

        [HttpPost]
        public ActionResult<Sale> Create(Sale sale)
        {
            // Validar se o User existe
            var user = _context.Users.Find(sale.UserID);
            if (user == null) return BadRequest("User not found");
            sale.User = user;

            // Nota: Os produtos devem ser geridos com cuidado para não duplicar, 
            // mas para este exercício simples, assumimos que vêm corretos ou vazios.
            
            _context.Sales.Add(sale);
            _context.SaveChanges();
            
            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Sale updatedSale)
        {
            var existing = _context.Sales.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            existing.Description = updatedSale.Description;
            existing.TotalPrice = updatedSale.TotalPrice;
            
            // Atualizar relações é mais complexo no EF Core, 
            // mas para campos simples isto basta:
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _context.Sales.FirstOrDefault(s => s.Id == id);
            if (existing == null) return NotFound();

            _context.Sales.Remove(existing);
            _context.SaveChanges();
            return NoContent();
        }
    }
}