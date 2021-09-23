using lestoma.App.ItemViewModels;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Essentials;

namespace lestoma.App.ViewModels.Buzon
{
    public class BuzonDeReportesViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private bool _isRunning;
        private ObservableCollection<BuzonItemViewModel> _reportesDelBuzonView;
        private List<BuzonDTO> _reportesDelBuzon;

        public BuzonDeReportesViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            Title = "Listado de buzón de reportes";
            LoadBuzonDeReportesAsync();
        }

        public ObservableCollection<BuzonItemViewModel> ReportesDelBuzonView
        {
            get => _reportesDelBuzonView;
            set => SetProperty(ref _reportesDelBuzonView, value);
        }
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private async void LoadBuzonDeReportesAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                return;
            }

            IsRunning = true;
            string url = App.Current.Resources["UrlAPI"].ToString();
            TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
            Response response = await _apiService.GetListAsyncWithToken<List<BuzonDTO>>(url, "ReportsMailbox/listado",
                UserApp.Token);

            IsRunning = false;

            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }

            _reportesDelBuzon = (List<BuzonDTO>)response.Data;
            MostrarReportes();
        }

        private void MostrarReportes()
        {
            ReportesDelBuzonView = new ObservableCollection<BuzonItemViewModel>(_reportesDelBuzon.Select(p => new BuzonItemViewModel(_navigationService)
            {
                Detalle = p.Detalle,
                User = p.User,
                FechaCreacion = p.FechaCreacion,
                Id = p.Id
            }).ToList());
        }
    }
}
