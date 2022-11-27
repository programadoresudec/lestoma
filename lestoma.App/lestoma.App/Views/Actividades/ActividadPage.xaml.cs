using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.Views.Actividades
{
    public partial class ActividadPage : ContentPage
    {

        public ActividadPage()
        {
            InitializeComponent();
        }


        private async Task WaitAndExecute(int milisec, Action actionToExecute)
        {
            await Task.Delay(milisec);
            actionToExecute();
        }
    }
}
