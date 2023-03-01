using lestoma.Entidades.Models;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string TramaEnviada { get; set; }
        public string TramaRecibida { get; set; }
        public bool EstadoInternet { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public string Ip { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public string Session { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public string TipoDeAplicacion { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public DateTime FechaCreacionServer { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public DateTime FechaCreacionDispositivo { get; set; }
    }
}
