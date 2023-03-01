using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    public class LaboratorioOffline
    {
        [Column("fecha_creacion_dispositivo")]
        public string FechaCreacionDispositivo { get; set; }
    }
}
