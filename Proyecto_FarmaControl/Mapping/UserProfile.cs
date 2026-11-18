
using AutoMapper;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterRequestDto, Usuario>()
                .ForMember(
                    d => d.UserName,
                    opt => opt.MapFrom(src => src.Username))
                .ForMember(
                    d => d.Password,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.EmailConfirmed,
                    opt => opt.MapFrom(src => false))
                .ForMember(
                    d => d.EmailConfirmToken,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.RefreshToken,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.RefreshTokenExpiryTime,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.PasswordResedToken,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.PasswordResetTokenExpirityTime,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.Role,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.RoleId,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.UsuarioPermisos,
                    opt => opt.Ignore());
        }
    }
}
