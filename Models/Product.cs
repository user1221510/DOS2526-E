using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProductsAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; } // Decimal é obrigatório para dinheiro

        [JsonIgnore]
        public List<Supplier> Suppliers { get; set; } = new();

        [JsonIgnore]
        public List<Sale> Sales { get; set; } = new();
    }
}