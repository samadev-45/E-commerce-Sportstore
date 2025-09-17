using AutoMapper;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcommerceAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map RegisterDto -> User (we will set PasswordHash manually after mapping)
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "user"))
                .ForMember(dest => dest.IsBlock, opt => opt.MapFrom(src => false));
        }
    }
}
