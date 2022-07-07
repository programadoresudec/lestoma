using lestoma.App.ViewModels.Modulos;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.Views.Modulos
{
    public partial class ModuloPage : ContentPage
    {
        private readonly INavigationService _navigation;
        private readonly IApiService _apiService;

        ModuloViewModel _viewModel;
        public ModuloPage(INavigationService navigationService, IApiService apiService)
        {
            _navigation = navigationService;
            _apiService = apiService;
            InitializeComponent();
            _viewModel = new ModuloViewModel(_navigation, _apiService);
        }
        private void LV_Actividades_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if (e.SwipeOffset >= 100)
            {
                _viewModel.ItemDelete = e.ItemData as ModuloDTO;
            }
        }

        private void BT_eliminar_Clicked(object sender, System.EventArgs e)
        {
            _viewModel.DeleteClicked();
        }
    }
}
