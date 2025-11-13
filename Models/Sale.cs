using System.Collections.Generic;

namespace ProductsAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double TotalPrice { get; set; }
        public List<Product> Products { get; set; } = new();

        // Relação: Uma Sale está associada a um User (Chave Estrangeira e Propriedade de Navegação)
        public int UserID { get; set; }
        public User User { get; set; }
    }
}