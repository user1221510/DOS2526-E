using System.Collections.Generic;

namespace ProductsAPI.Models
{
    public class Product
    {
        // Campos existentes
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        
        // Relação: Um Product tem vários Suppliers
        public List<Supplier> Suppliers { get; set; } = new(); 
    }
}