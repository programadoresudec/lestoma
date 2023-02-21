using System;
using System.Collections.Generic;
using System.Text;

namespace lestoma.App.Models
{
    public class FiltroFechaModel
    {
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public TimeSpan HoraInicial { get; set; }
        public TimeSpan HoraFinal { get; set; }
    }
    public class FilterDate
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public DateTime ConcatenateDateTime(DateTime fecha, TimeSpan hora)
        {
            return fecha.Add(hora);
        }
    }
}
