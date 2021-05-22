using AutoMapper;
using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Responses;

namespace lestoma.Api.Helpers
{
    public class AutoMappersProfiles : Profile
    {
        public AutoMappersProfiles()
        {
            CreateMap<UsuarioRequest, EUsuario>();
            CreateMap<EUsuario, UserResponse>().ForMember(d => d.RolId, o => o.MapFrom(s => s.Rol.Id));
        }
    }
}
