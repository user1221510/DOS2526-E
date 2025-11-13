using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Models;
using System.Collections.Generic;
using System.Linq;
using ProductsAPI.Controllers; // Necessário para aceder ao modelo Supplier

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // É necessário criar um Supplier mock para satisfazer a nova relação
        private static readonly Supplier MockSupplier = new Supplier { Id = 1, Name = "Tech Components Ltda", Email = "contato@techcomp.com" };

        // Para exemplo simples: usar uma lista em memória
        private static readonly List<Product> _products = new()
        {
            new Product 
            { 
                Id = 1, 
                Name = "Mouse", 
                Price = 25.50M,
                // Adição da nova propriedade de relação
                Suppliers = new List<Supplier> { MockSupplier }
            },
            new Product 
            { 
                Id = 2, 
                Name = "Teclado", 
                Price = 45.20M,
                // Adição da nova propriedade de relação
                Suppliers = new List<Supplier> { MockSupplier }
            }
        };

        // ... (Endpoints GetAll, GetById, Create, Update, Delete mantidos)

        [HttpPut("{id}")]
        public IActionResult Update(int id, Product product)
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = product.Name;
            existing.Price = product.Price;
            
            // Atualizando as propriedades de relação
            existing.Suppliers = product.Suppliers; 
            
            return NoContent();  // 204
        }
    }
}