using MyApp.DTOs.Cart;
using MyApp.DTOs.Orders;
using MyApp.DTOs.Wishlist;

namespace MyApp.DTOs.Auth
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        
    }
}   