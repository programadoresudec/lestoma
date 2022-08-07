using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    [Table("actividad")]
    public class ActividadOffline : CamposAuditoriaOffline
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Column("nombre_actividad")]
        public string Nombre { get; set; }
    }
}
