using AutoMapper;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.Models;
using lestoma.Entidades.Models;

namespace lestoma.DatabaseOffline
{
    public class Mapper
    {
        public static IMapper CreateMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ActividadModel, ActividadRequest>().ReverseMap();
            });

            return mapperConfiguration.CreateMapper();
        }
    }
}
