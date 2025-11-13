using System.Collections.Generic;

namespace ProductsAPI.Models
{
    public class Supplier
    {
        // Campos existentes
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // Relação: Um Supplier pode fornecer vários Products
        public List<Product> Products { get; set; } = new();
    }
}