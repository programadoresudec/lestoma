using lestoma.App.ViewModels.Upas;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.Views.Upas
{
    public partial class UpaPage : ContentPage
    {
        private readonly INavigationService _navigation;
        private readonly IApiService _apiService;
        UpaViewModel _viewModel;
        public UpaPage(INavigationService navigationService, IApiService apiService)
        {
            _navigation = navigationService;
            _apiService = apiService;
            InitializeComponent();
            _viewModel = new UpaViewModel(_navigation, _apiService);
        }

        private void LV_upas_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if (e.SwipeOffset >= 100)
            {
                _viewModel.ItemDelete = e.ItemData as UpaDTO;
            }
        }

        private void Bt_eliminar_Clicked(object sender, System.EventArgs e)
        {
            _viewModel.DeleteClicked();
        }
    }
}
