using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private static readonly User MockUser = new User { Id = 1, Username = "joaos", Email = "joao@example.com", FullName = "João Silva", Role = "Admin" };

        private static readonly List<Sale> _sales = new()
        {
            new Sale
            {
                Id = 1,
                Description = "Venda 1",
                TotalPrice = 70.70,
                Products = new List<Product>
                {
                    new Product { Id = 1, Name = "Mouse", Price = 25.50M },
                    new Product { Id = 2, Name = "Teclado", Price = 45.20M }
                },
                UserID = MockUser.Id, 
                User = MockUser 
            }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Sale>> GetSales()
        {
            return Ok(_sales);
        }

        [HttpGet("{id}")]
        public ActionResult<Sale> GetSale(int id)
        {
            var sale = _sales.FirstOrDefault(s => s.Id == id);
            if (sale == null)
                return NotFound();
            return Ok(sale);
        }

        [HttpPost]
        public ActionResult<Sale> Create(Sale sale)
        {
            sale.Id = _sales.Any() ? _sales.Max(s => s.Id) + 1 : 1;
            sale.Products ??= new List<Product>();
            
            if (sale.User != null)
            {
                sale.UserID = sale.User.Id;
                sale.User = MockUser; 
            }
            
            _sales.Add(sale);
            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Sale updatedSale)
        {
            var existing = _sales.FirstOrDefault(s => s.Id == id);
            if (existing == null)
                return NotFound();

            existing.Description = updatedSale.Description;
            existing.TotalPrice = updatedSale.TotalPrice;
            existing.Products = updatedSale.Products;
            
            // Atualiza as propriedades de relação
            existing.UserID = updatedSale.UserID;
            existing.User = updatedSale.User;

            return NoContent();
        }

         [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _sales.FirstOrDefault(s => s.Id == id);
            if (existing == null)
                return NotFound();

            _sales.Remove(existing);
            return NoContent();
        }
    }
}