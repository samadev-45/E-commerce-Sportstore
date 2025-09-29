using MyApp.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Net.Http;

namespace MyApp.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            // Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Name = "Cricket Bat", Price = 1499, Category = "Cricket", Description = "High quality cricket bat" },
                    new Product { Name = "Nivia Storm Football", Price = 999, Category = "Football", Description = "Official size football" },
                    new Product { Name = "Tennis Racket", Price = 1999, Category = "Tennis", Description = "Lightweight tennis racket" },
                    new Product { Name = "Basketball", Price = 1099, Category = "Basketball", Description = "Indoor/outdoor basketball" },
                    new Product { Name = "Gym Gloves", Price = 499, Category = "Gym", Description = "Comfortable gym gloves" },
                    new Product { Name = "Real Madrid Jersey", Price = 1299, Category = "Football", Description = "Official Real Madrid jersey" },
                    new Product { Name = "FC Barcelona Jersey", Price = 1199, Category = "Football", Description = "Official FC Barcelona jersey" },
                    new Product { Name = "Nike Running Shoes", Price = 5299, Category = "Shoes", Description = "Lightweight running shoes" },
                    new Product { Name = "Adidas Sneakers", Price = 3499, Category = "Shoes", Description = "Casual sneakers for everyday wear" },
                    new Product { Name = "Cricket Helmet", Price = 2499, Category = "Cricket", Description = "Protective cricket helmet" }
                };

                context.Products.AddRange(products);
                context.SaveChanges();

                // Add images for products
                var httpClient = new HttpClient();

                var productImageUrls = new Dictionary<string, string>
                {
                    { "Cricket Bat", "https://m.media-amazon.com/images/I/6164TY9DCtL._SX679_.jpg" },
                    { "Nivia Storm Football", "https://m.media-amazon.com/images/I/61RpRYFb2wL._SL1100_.jpg" },
                    { "Tennis Racket", "https://scssports.in/cdn/shop/files/Burn_Pro_PS13_Badminton_Racket_d1da6f79-7a3f-4161-961d-1d72abce3bac.jpg?v=1735299406" },
                    { "Basketball", "https://i.pinimg.com/1200x/ac/52/ff/ac52ff038a1375463bc139df779b2bf2.jpg" },
                    { "Gym Gloves", "https://m.media-amazon.com/images/I/51lFRJd4YIL._SX300_SY300_QL70_FMwebp_.jpg" },
                    { "Real Madrid Jersey", "https://i.pinimg.com/736x/f6/8f/3d/f68f3d846e8a6a71d2241585732b4b34.jpg" },
                    { "FC Barcelona Jersey", "https://i.pinimg.com/736x/c0/61/1c/c0611c1d1c7b545139986ed0545eaeb9.jpg" },
                    { "Nike Running Shoes", "https://i.pinimg.com/1200x/1e/84/13/1e8413a955ff34335c957f0c5a3e3bea.jpg" },
                    { "Adidas Sneakers", "https://m.media-amazon.com/images/I/618RT+-tqHL._AC_SR230,210_CB1169409_QL70_.jpg" },
                    { "Cricket Helmet", "https://m.media-amazon.com/images/I/61hS2F+zJUL._SX679_.jpg" }
                };

                foreach (var product in products)
                {
                    if (productImageUrls.TryGetValue(product.Name, out var imageUrl))
                    {
                        var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;
                        var extension = Path.GetExtension(imageUrl).TrimStart('.');

                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            FileName = Path.GetFileNameWithoutExtension(imageUrl),
                            FileExtension = extension,
                            FileData = imageBytes
                        };

                        context.Set<ProductImage>().Add(productImage);
                    }
                }

                context.SaveChanges();
            }

            // Seed Users (unchanged)
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User { Name = "sam", Email = "abc@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "user", IsBlock = false },
                    new User { Name = "Admin", Email = "admin@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "admin", IsBlock = false },
                    new User { Name = "Ashfaque", Email = "asdf@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("asdfghjkl"), Role = "user", IsBlock = false }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}
