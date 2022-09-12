using lestoma.App.Views.Buzon;
using lestoma.App.Views.UpasActividades;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Buzon
{
    public class BuzonDeReportesViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private bool _isRunning;
        private ObservableCollection<BuzonDTO> _reportesDelBuzon;
        private Command<object> itemTap;
        public BuzonDeReportesViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
            Title = "Listado de buzón de reportes";
            SeeMoreInfoCommand = new Command<object>(OnSeeMoreInfo, CanNavigate);
            LoadBuzonDeReportesAsync();
        }

        public ObservableCollection<BuzonDTO> ReportesDelBuzon
        {
            get => _reportesDelBuzon;
            set => SetProperty(ref _reportesDelBuzon, value);
        }
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }
        public Command<object> SeeMoreInfoCommand
        {
            get => itemTap;
            set => SetProperty(ref itemTap, value);
        }
        private bool CanNavigate(object arg)
        {
            return true;
        }
        private async void OnSeeMoreInfo(object obj)
        {
            BuzonDTO buzon = (BuzonDTO)obj;
            if (buzon == null)
                return;
            var parameters = new NavigationParameters
            {
                { "BuzonId",  buzon.Id}
            };
            await _navigationService.NavigateAsync($"{nameof(MoreInfoPopupPage)}", parameters);
        }


        private async void LoadBuzonDeReportesAsync()
        {
            PageSize = 100;
            try
            {
                if (_apiService.CheckConnection())
                {
                    IsRunning = true;
                    Response response = await _apiService.GetPaginadoAsyncWithToken<BuzonDTO>(URL,
                        $"buzon-de-reportes/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertError(response.Mensaje);
                        return;
                    }
                    Paginador<BuzonDTO> paginador = (Paginador<BuzonDTO>)response.Data;
                    ReportesDelBuzon = new ObservableCollection<BuzonDTO>(paginador.Datos);
                    IsRunning = false;
                }
                else
                {
                    AlertNoInternetConnection();
                }
            }
            catch (Exception ex)
            {
                IsRunning = false;
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
