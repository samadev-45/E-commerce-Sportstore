using AutoMapper;
using MyApp.DTOs.Admin;
using MyApp.DTOs.Auth;
using MyApp.DTOs.Cart;
using MyApp.DTOs.Orders;
using MyApp.DTOs.Products;
using MyApp.DTOs.Wishlist;
using MyApp.Entities;
using System;

namespace MyApp.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // -----------------------
            // Product mappings
            // -----------------------
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();
            CreateMap<ProductFilterDto, Product>();

            // -----------------------
            // Cart mappings
            // -----------------------
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));
            CreateMap<AddToCartDto, CartItem>();

            // -----------------------
            // Wishlist mappings
            // -----------------------
            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));
            CreateMap<AddToWishlistDto, WishlistItem>();

            // -----------------------
            // Order mappings
            // -----------------------
            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.PaymentType == "COD" ? "Pending" : "Payment Initiated"))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore()) 
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore()); // Razorpay 
                

            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

            CreateMap<Order, AdminOrderDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => $"order_{src.Id}"))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.OrderDate));

            CreateMap<OrderItem, AdminOrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));

            // -----------------------
            // User mappings
            // -----------------------
            CreateMap<RegisterDto, User>();
            CreateMap<User, UserProfileDto>();
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlock));
        }
    }
}
