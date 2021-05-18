using AutoMapper;
using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Requests;

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
