using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lestoma.App.Views.UpasActividades
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetalleUpaActividadPage : ContentPage
    {
        public DetalleUpaActividadPage()
        {
            InitializeComponent();
        }

        private void LV_detalleUpasActividades_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {

        }
    }
}