using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProductsAPI.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }

        [JsonIgnore]
        public List<Product> Products { get; set; } = new();
    }
}