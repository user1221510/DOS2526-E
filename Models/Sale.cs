using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProductsAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal TotalPrice { get; set; } // Alterado para Decimal

        public List<Product> Products { get; set; } = new();

        public int UserID { get; set; }
        public User? User { get; set; }
    }
}