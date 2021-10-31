using lestoma.Entidades.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.DatabaseOffline.Models
{
    [Table("actividad")]
    public class ActividadModel : ECamposAuditoria
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Column("nombre_actividad")]
        public string Nombre { get; set; }
    }
}
