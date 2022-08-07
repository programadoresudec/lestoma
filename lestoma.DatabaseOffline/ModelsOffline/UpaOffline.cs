using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    [Table("upa")]
    public class UpaOffline : CamposAuditoriaOffline
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Column("nombre_actividad")]
        public string Nombre { get; set; }
    }
}
