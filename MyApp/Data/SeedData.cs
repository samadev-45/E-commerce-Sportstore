using MyApp.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;


namespace MyApp.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            //  Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Name = "Cricket Bat", Price = 1499, Category = "Cricket", ImageUrl = "https://m.media-amazon.com/images/I/6164TY9DCtL._SX679_.jpg" },
                    new Product { Name = "Nivia Storm Football - Size 5 (White), Rubber", Price = 999, Category = "Football", ImageUrl = "https://m.media-amazon.com/images/I/61RpRYFb2wL._SL1100_.jpg" },
                    new Product { Name = "Tennis Racket", Price = 1999, Category = "Tennis", ImageUrl = "https://scssports.in/cdn/shop/files/Burn_Pro_PS13_Badminton_Racket_d1da6f79-7a3f-4161-961d-1d72abce3bac.jpg?v=1735299406" },
                    new Product { Name = "Bacca Bucci Men Lace Up Basketball Shoe", Price = 2999, Category = "Basketball", ImageUrl = "https://i.pinimg.com/736x/98/f0/92/98f092f727de22ad7662fdf8bff44325.jpg" },
                    new Product { Name = "Gym Gloves", Price = 499, Category = "Gym", ImageUrl = "https://m.media-amazon.com/images/I/51lFRJd4YIL._SX300_SY300_QL70_FMwebp_.jpg" },
                    new Product { Name = "Real Madrid Jersey", Price = 1299, Category = "Football", ImageUrl = "https://i.pinimg.com/736x/f6/8f/3d/f68f3d846e8a6a71d2241585732b4b34.jpg" },
                    new Product { Name = "FC Barcelona Jersey", Price = 1199, Category = "Football", ImageUrl = "https://i.pinimg.com/736x/c0/61/1c/c0611c1d1c7b545139986ed0545eaeb9.jpg" },
                    new Product { Name = "Nike Football Shoe", Price = 5299, Category = "Football", ImageUrl = "https://i.pinimg.com/1200x/1e/84/13/1e8413a955ff34335c957f0c5a3e3bea.jpg" },
                    new Product { Name = "Adidas Running Shoes", Price = 3499, Category = "Shoes", ImageUrl = "https://m.media-amazon.com/images/I/618RT+-tqHL._AC_SR230,210_CB1169409_QL70_.jpg" },
                    new Product { Name = "Cricket Shoes", Price = 1799, Category = "Cricket", ImageUrl = "https://i.pinimg.com/736x/53/d1/51/53d1518226ec8f158ee1d6d90c56c313.jpg" },
                    new Product { Name = "Chelsea Club Jersey", Price = 1399, Category = "Football", ImageUrl = "https://m.media-amazon.com/images/I/71ccn1Ko-rL._SX679_.jpg" },
                    new Product { Name = "Cricket Socks", Price = 199, Category = "Cricket", ImageUrl = "https://m.media-amazon.com/images/I/41JXNK8dAHL._AC_SR250,250_QL65_.jpg" },
                    new Product { Name = "Gym Shoes", Price = 2199, Category = "Gym", ImageUrl = "https://m.media-amazon.com/images/I/41jd1iAMdTL._SX300_SY300_QL70_FMwebp_.jpg" },
                    new Product { Name = "Manchester United Jersey", Price = 1399, Category = "Football", ImageUrl = "https://i.pinimg.com/1200x/6e/72/52/6e725260e8cc26ed60569ec2643c60cd.jpg" },
                    new Product { Name = "Basketball Socks", Price = 249, Category = "Basketball", ImageUrl = "https://i.pinimg.com/1200x/f4/4f/f2/f44ff213d5719e51221b5bb657717747.jpg" },
                    new Product { Name = "Football Socks", Price = 229, Category = "Football", ImageUrl = "https://i.pinimg.com/736x/80/89/ca/8089caaf9f5ad6e7117323e6c73de2a7.jpg" },
                    new Product { Name = "Goalkeeper Gloves", Price = 799, Category = "Football", ImageUrl = "https://i.pinimg.com/736x/cd/6e/9c/cd6e9c4a43b8cd7c67a87971bbca9ac0.jpg" },
                    new Product { Name = "Basketball", Price = 1099, Category = "Basketball", ImageUrl = "https://i.pinimg.com/1200x/ac/52/ff/ac52ff038a1375463bc139df779b2bf2.jpg" },
                    new Product { Name = "Basketball Jersey", Price = 1299, Category = "Basketball", ImageUrl = "https://i.pinimg.com/1200x/01/d1/9f/01d19fced313b21980f2c362460cea4f.jpg" },
                    new Product { Name = "Gym Shorts", Price = 899, Category = "Gym", ImageUrl = "https://i.pinimg.com/1200x/64/fc/dd/64fcdd2f4a06ab6bf21eb7031ab5decf.jpg" },
                    new Product { Name = "Training Cones Set", Price = 599, Category = "Football", ImageUrl = "https://i.pinimg.com/736x/8a/da/37/8ada3794ed622a1e1f9de2b738626385.jpg" },
                    new Product { Name = "Tennis Balls Pack of 3", Price = 399, Category = "Tennis", ImageUrl = "https://i.pinimg.com/736x/04/3f/d9/043fd924b0422c2370ec97e256b516b0.jpg" },
                    new Product { Name = "Basketball Arm Sleeves", Price = 349, Category = "Basketball", ImageUrl = "https://i.pinimg.com/1200x/06/43/05/064305955168a894728ff4ea919235b2.jpg" },
                    new Product { Name = "Asco Dream Football shoes", Price = 1799, Category = "Football", ImageUrl = "https://i.pinimg.com/736x/41/cf/02/41cf029e2ff49f68ccaec1d649100d9f.jpg", Description = "Synthetic leather upper, durable fit with rubber sole" }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Name = "sam",
                        Email = "abc@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Role = "user",
                        IsBlock = false
                    },
                    new User
                    {
                        Name = "Admin",
                        Email = "admin@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = "admin",
                        IsBlock = false
                    },
                    new User
                    {
                        Name = "Ashfaque",
                        Email = "asdf@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("asdfghjkl"),
                        Role = "user",
                        IsBlock = false
                    },
                    new User
                    {
                        Name = "adnan",
                        Email = "ad@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("adnan123"),
                        Role = "user",
                        IsBlock = false
                    },
                    new User
                    {
                        Name = "akshay",
                        Email = "ak@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ak2123"),
                        Role = "user",
                        IsBlock = false
                    },
                    new User
                    {
                        Name = "Devan",
                        Email = "dev@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("dev123"),
                        Role = "user",
                        IsBlock = false
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}
