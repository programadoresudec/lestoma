using System;
using System.ComponentModel.DataAnnotations;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    public class ComponenteOffline
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UpaId { get; set; }
        public Guid ActividadId { get; set; }
        public Guid ModuloId { get; set; }
        public Guid ComponenteId { get; set; }
        public string NombreUpa { get; set; }
        public string NombreActividad { get; set; }
        public string NombreModulo { get; set; }
        public string NombreComponente { get; set; }
        public string Protocolos { get; set; }
        public string DecripcionEstadoJson { get; set; }
        public string DireccionRegistro { get; set; }

    }
}
