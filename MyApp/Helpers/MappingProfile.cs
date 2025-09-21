using AutoMapper;
using MyApp.DTOs.Auth;
using MyApp.DTOs.Cart;
using MyApp.DTOs.Orders;
using MyApp.DTOs.Products;
using MyApp.DTOs.Wishlist;
using MyApp.Entities;

namespace MyApp.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings
            CreateMap<Product, ProductDto>(); // For GET requests (read-only)
            CreateMap<ProductDto, Product>()  // For POST/PUT (write)
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ProductFilterDto, Product>();

            // Cart mappings
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));
            CreateMap<AddToCartDto, CartItem>();

            // Wishlist mapping
            CreateMap<WishlistItem, WishlistItemDto>()
               .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));
            CreateMap<AddToWishlistDto, WishlistItem>();

            // Orders
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.Product.Name));

            CreateMap<RegisterDto, User>();
            CreateMap<User, UserProfileDto>();
        }
    }
}
