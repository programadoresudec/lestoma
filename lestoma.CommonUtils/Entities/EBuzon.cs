using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.CommonUtils.Entities
{
    [Table("reportes", Schema = "buzon")]
    public class EBuzon
    {

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
        [Column("descripcion")]
        public string Descripcion { get; set; }
        [Column("usuario_id")]
        public int UsuarioId { get; set; }
    }
}
