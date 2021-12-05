using lestoma.App.ViewModels.Actividades;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.Views.Actividades
{
    public partial class ActividadPage : ContentPage
    {
        private readonly INavigationService _navigation;
        private readonly IApiService _apiService;

        ActividadViewModel _viewModel;
        public ActividadPage(INavigationService navigationService, IApiService apiService)
        {
            _navigation = navigationService;
            _apiService = apiService;
            InitializeComponent();
            _viewModel = new ActividadViewModel(_navigation, _apiService);

        }
        private void LV_Actividades_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if (e.SwipeOffset >= 100)
            {
                _viewModel.ItemDelete = e.ItemData as ActividadDTO;
            }
        }

        private void BT_eliminar_Clicked(object sender, System.EventArgs e)
        {
            _viewModel.DeleteClicked();
        }
    }
}
