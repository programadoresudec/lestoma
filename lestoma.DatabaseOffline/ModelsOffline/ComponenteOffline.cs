using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lestoma.DatabaseOffline.ModelsOffline
{
    public class ComponenteOffline
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Column("nombre_actividad")]
        public string NombreActividad { get; set; }
    }
}
