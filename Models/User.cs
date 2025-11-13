using System.Collections.Generic;

namespace ProductsAPI.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Username { get; set; } 
        public string Email { get; set; } 
        public string FullName { get; set; } 
        public string Role { get; set; } 
        
        // Relação: Um User pode ter várias Sales
        public List<Sale> Sales { get; set; } = new(); 
    }
}