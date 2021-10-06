using AutoMapper;
using lestoma.App.ViewModels.Actividades;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.Interfaces;
using lestoma.DatabaseOffline.Logica;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.Views.Actividades
{
    public partial class ActividadPage : ContentPage
    {
        private readonly INavigationService _navigation;
        private readonly IApiService _apiService;

        ActividadPageViewModel _viewModel;
        public ActividadPage(INavigationService navigationService, IApiService apiService)
        {
            _navigation = navigationService;
            _apiService = apiService;
            InitializeComponent();
            _viewModel = new ActividadPageViewModel(_navigation, _apiService);

        }
        private void button_Clicked(object sender, System.EventArgs e)
        {
            _viewModel.DeleteClicked();
        }

        private void LV_Actividades_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if (e.SwipeOffset >= 100)
            {
                _viewModel.ItemDelete = e.ItemData as ActividadRequest;
            }
        }
    }
}
