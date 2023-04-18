using AutoMapper;
using lestoma.CommonUtils.DTOs.Sync;
using lestoma.DatabaseOffline.ModelsOffline;
using Newtonsoft.Json;

namespace lestoma.DatabaseOffline
{
    public class Mapper
    {
        public static IMapper CreateMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DataOnlineSyncDTO, ComponenteOffline>()
                .ForMember(dest => dest.UpaId, opt => opt.MapFrom(src => src.Upa.Id))
                .ForMember(dest => dest.ActividadId, opt => opt.MapFrom(src => src.Actividad.Id))
                .ForMember(dest => dest.ModuloId, opt => opt.MapFrom(src => src.Modulo.Id))
                .ForMember(dest => dest.ComponenteId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NombreUpa, opt => opt.MapFrom(src => src.Upa.Nombre))
                .ForMember(dest => dest.NombreActividad, opt => opt.MapFrom(src => src.Actividad.Nombre))
                .ForMember(dest => dest.NombreModulo, opt => opt.MapFrom(src => src.Modulo.Nombre))
                .ForMember(dest => dest.NombreComponente, opt => opt.MapFrom(src => src.NombreComponente))
                .ForMember(dest => dest.Protocolos, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Protocolos)))
                .ForMember(dest => dest.DecripcionEstadoJson, opt => opt.MapFrom(src => src.DescripcionEstadoJson))
                .ForMember(dest => dest.DireccionRegistro, opt => opt.MapFrom(src => src.DireccionRegistro));
            });

            return mapperConfiguration.CreateMapper();
        }
    }
}
