using lestoma.CommonUtils.DTOs.Sync;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.ModelsOffline;
using Mapster;
using MapsterMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace lestoma.DatabaseOffline
{
    public static class MyMapperConfig
    {
        public static void Register()
        {
            TypeAdapterConfig<DataOnlineSyncDTO, ComponenteOffline>
                .NewConfig()
              .Map(dest => dest.UpaId, src => src.Upa.Id)
              .Map(dest => dest.ActividadId, src => src.Actividad.Id)
              .Map(dest => dest.ModuloId, src => src.Modulo.Id)
              .Map(dest => dest.ComponenteId, src => src.Id)
              .Map(dest => dest.NombreUpa, src => src.Upa.Nombre)
              .Map(dest => dest.NombreActividad, src => src.Actividad.Nombre)
              .Map(dest => dest.NombreModulo, src => src.Modulo.Nombre)
              .Map(dest => dest.NombreComponente, src => src.NombreComponente)
              .Map(dest => dest.Protocolos, src => JsonConvert.SerializeObject(src.Protocolos))
              .Map(dest => dest.DecripcionEstadoJson, src => src.DescripcionEstadoJson)
              .Map(dest => dest.DireccionRegistro, src => src.DireccionRegistro);

            TypeAdapterConfig<LaboratorioOffline, LaboratorioRequest>
               .NewConfig()
               .Map(dest => dest.ComponenteId, src => src.ComponenteLaboratorioId)
               .Map(dest => dest.SetPointIn, src => src.ValorCalculadoTramaEnviada)
               .Map(dest => dest.SetPointOut, src => src.ValorCalculadoTramaRecibida);
        }
    }
}
