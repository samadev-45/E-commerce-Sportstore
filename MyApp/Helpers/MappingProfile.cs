using AutoMapper;
using MyApp.DTOs.Admin;
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
            CreateMap<Product, ProductDto>(); // For GET requests (read-only)
            CreateMap<ProductDto, Product>()  // For POST/PUT (write)
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ProductCreateDto, Product>();  // <--- ADD THIS
            CreateMap<ProductUpdateDto, Product>();  // <--- ADD THIS
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

            // Map Order -> AdminOrderDto
            CreateMap<Order, AdminOrderDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => $"order_{src.Id}"))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.OrderDate));

            // Map OrderItem -> AdminOrderItemDto
            CreateMap<OrderItem, AdminOrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));

            CreateMap<RegisterDto, User>();
            CreateMap<User, UserProfileDto>();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlock));
        }
    }
}
