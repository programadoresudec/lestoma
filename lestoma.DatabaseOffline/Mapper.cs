using AutoMapper;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.ModelsOffline;

namespace lestoma.DatabaseOffline
{
    public class Mapper
    {
        public static IMapper CreateMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ActividadOffline, ActividadRequest>().ReverseMap();
                cfg.CreateMap<ActividadOffline, ActividadDTO>().ReverseMap();
            });

            return mapperConfiguration.CreateMapper();
        }
    }
}
