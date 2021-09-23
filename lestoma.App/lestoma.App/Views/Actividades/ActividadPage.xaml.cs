using lestoma.App.ViewModels.Actividades;
using lestoma.CommonUtils.Requests;
using Xamarin.Forms;

namespace lestoma.App.Views.Actividades
{
    public partial class ActividadPage : ContentPage
    {
        ActividadPageViewModel _viewModel;
        public ActividadPage()
        {
            InitializeComponent();
        }

        private void LV_Actividades_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if (e.SwipeOffset >= 360)
            {
                var objeto = e.ItemData;
                _viewModel.ItemDelete = objeto as ActividadRequest;
            }
        }
    }
}
