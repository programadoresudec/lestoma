using lestoma.Entidades.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.DatabaseOffline.Models
{
    [Table("actividad")]
    public class ActividadModel : ECamposAuditoria
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("nombre_actividad")]
        public string Nombre { get; set; }
    }
}
