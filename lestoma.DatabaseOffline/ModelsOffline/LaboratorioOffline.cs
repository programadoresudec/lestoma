using System;
using System.ComponentModel.DataAnnotations;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    public class LaboratorioOffline
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public Guid ComponenteLaboratorioId { get; set; }
        public double? ValorCalculadoTramaEnviada { get; set; }
        public double? ValorCalculadoTramaRecibida { get; set; }
        [Required(ErrorMessage = "Trama enviada requerido.")]
        public string TramaEnviada { get; set; }
        [Required(ErrorMessage = "trama recibida requerido.")]
        public string TramaRecibida { get; set; }
        public string Session { get; set; }
        public string TipoDeAplicacion { get; set; }
        public bool EstadoInternet => false;
        public DateTime FechaCreacionDispositivo { get; set; }
        public bool IsMigrated { get; set; } = false;
    }
}
