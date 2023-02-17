using System;
using System.Collections.Generic;
using System.Text;

namespace lestoma.App.Models
{
    public class FiltroFechaModel
    {
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public string HoraInicial { get; set; }
        public string HoraFinal { get; set; }
        public DateTime FechaInicio => FechaInicial.Add(TimeSpan.Parse(HoraInicial));
        public DateTime FechaFin =>  FechaFinal.Add(TimeSpan.Parse(HoraFinal));
    }
}
