using System;

namespace lestoma.App.Models
{
    public class ComponenteCheckModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public bool IsChecked { get; set; }
    }
}
