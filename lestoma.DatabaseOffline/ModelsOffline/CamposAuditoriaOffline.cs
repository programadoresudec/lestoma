using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    public class CamposAuditoriaOffline
    {
        [Column("ip")]
        public string Ip { get; set; }
        [Column("session")]
        public string Session { get; set; }
        [Column("tipo_de_aplicacion")]
        public string TipoDeAplicacion { get; set; }
        [Column("fecha_creacion")]
        public string FechaCreacionServer { get; set; }
    }
}
