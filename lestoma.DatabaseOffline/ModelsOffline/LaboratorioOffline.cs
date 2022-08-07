using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    public class LaboratorioOffline : CamposAuditoriaOffline
    {
        [Column("fecha_creacion_dispositivo")]
        public string FechaCreacionDispositivo { get; set; }
    }
}
