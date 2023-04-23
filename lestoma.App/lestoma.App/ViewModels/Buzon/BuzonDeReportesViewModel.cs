using Acr.UserDialogs;
using lestoma.App.Views.Buzon;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Buzon
{
    public class BuzonDeReportesViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<BuzonDTO> _reportesDelBuzon;
        private Command<object> itemTap;
        public BuzonDeReportesViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
            Title = "Listado de buzón de reportes";
            SeeMoreInfoCommand = new Command<object>(OnSeeMoreInfo, CanNavigate);
            MoreReportsCommand = new Command(LoadMoreBuzonDeReportesAsync);
            LoadBuzonDeReportesAsync();
        }

        private async void LoadMoreBuzonDeReportesAsync()
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (_reportesDelBuzon.Any())
                {
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<BuzonDTO>(URL_API,
                   $"buzon-de-reportes/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertError(response.MensajeHttp);
                        return;
                    }
                    Paginador<BuzonDTO> paginador = (Paginador<BuzonDTO>)response.Data;
                    IsRefreshing = paginador.HasNextPage;
                    foreach (var item in paginador.Datos)
                    {
                        ReportesDelBuzon.Add(item);
                    }
                }

            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                UserDialogs.Instance.HideLoading();
            }
        }

        public ObservableCollection<BuzonDTO> ReportesDelBuzon
        {
            get => _reportesDelBuzon;
            set => SetProperty(ref _reportesDelBuzon, value);
        }

        public Command MoreReportsCommand { get; set; }
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
            await NavigationService.NavigateAsync($"{nameof(MoreInfoPopupPage)}", parameters);
        }


        private async void LoadBuzonDeReportesAsync()
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                IsBusy = true;
                ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<BuzonDTO>(URL_API,
                    $"buzon-de-reportes/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertError(response.MensajeHttp);
                    return;
                }
                Paginador<BuzonDTO> paginador = (Paginador<BuzonDTO>)response.Data;
                if (paginador.TotalDatos == 0)
                {
                    AlertWarning("No hay reportes en el buzón");
                    return;
                }
                IsRefreshing = paginador.HasNextPage;
                ReportesDelBuzon = new ObservableCollection<BuzonDTO>(paginador.Datos);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                IsBusy = false;
            }
        }

    }
}
