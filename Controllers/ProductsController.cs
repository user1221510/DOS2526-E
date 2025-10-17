using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // Para exemplo simples: usar uma lista em memória
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Mouse", Price = 25.50M },
            new Product { Id = 2, Name = "Teclado", Price = 45.20M }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            return Ok(_products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var prod = _products.FirstOrDefault(p => p.Id == id);
            if (prod == null)
                return NotFound();
            return Ok(prod);
        }

        [HttpPost]
        public ActionResult<Product> Create(Product product)
        {
            // definir novo Id (simples)
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
            // retornamos 201 Created com localização
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Product product)
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = product.Name;
            existing.Price = product.Price;
            return NoContent();  // 204
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null)
                return NotFound();

            _products.Remove(existing);
            return NoContent();
        }
    }
}
