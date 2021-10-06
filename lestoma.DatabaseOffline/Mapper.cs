using AutoMapper;
using lestoma.CommonUtils.Requests;
using lestoma.Entidades.Models;

namespace lestoma.DatabaseOffline
{
    public class Mapper
    {
        public static IMapper CreateMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EActividad, ActividadRequest>();
                cfg.CreateMap<ActividadRequest, EActividad>();
            });

            return mapperConfiguration.CreateMapper();
        }
    }
}
