using AutoMapper;
using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;

namespace lestoma.Api.Helpers
{
    public class AutoMappersProfiles : Profile
    {
        public AutoMappersProfiles()
        {
            CreateMap<UsuarioRequest, EUsuario>();
        }
    }
}
