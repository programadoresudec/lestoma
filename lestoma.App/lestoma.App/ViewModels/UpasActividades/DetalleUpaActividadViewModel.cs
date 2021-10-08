using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class DetalleUpaActividadViewModel : BaseViewModel
    {
        private INavigationService _navigationService;
        private IApiService _apiService;
        private ObservableCollection<DetalleUpaActividadDTO> _detalleUpaActividad;

        public DetalleUpaActividadViewModel(INavigationService navigationService, IApiService apiService) :
        base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _detalleUpaActividad = new ObservableCollection<DetalleUpaActividadDTO>();
            LoadDetalle();
            LoadMoreItemsCommand = new Command<object>(LoadMoreItems, CanLoadMoreItems);
        }

        public Command<object> LoadMoreItemsCommand { get; }
        public ObservableCollection<DetalleUpaActividadDTO> DetalleUpasActividades
        {
            get => _detalleUpaActividad;
            set => SetProperty(ref _detalleUpaActividad, value);
        }

        private async void LoadDetalle()
        {
            try
            {
                await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                if (!_apiService.CheckConnection())
                {
                    CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                    return;
                }
                ConsumoService();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                await _navigationService.ClearPopupStackAsync();
            }
        }

        private async void ConsumoService()
        {
            string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
            TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
            Response response = await _apiService.GetPaginadoAsyncWithToken<DetalleUpaActividadDTO>(url,
                $"UpasActividades/paginar?Page={Page}&&PageSize={PageSize}", UserApp.Token);
            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }
            Paginador<DetalleUpaActividadDTO> paginador = (Paginador<DetalleUpaActividadDTO>)response.Data;
            TotalItems = paginador.TotalDatos;
            if (paginador.HasNextPage)
            {
                Page += 1;
                if (DetalleUpasActividades.Count > 0)
                {
                    foreach (var item in paginador.Datos)
                    {
                        DetalleUpasActividades.Add(item);
                    }
                }
                else
                {
                    DetalleUpasActividades = new ObservableCollection<DetalleUpaActividadDTO>(paginador.Datos);
                }
            }
            else if (paginador.HasNextPage == false && paginador.HasPreviousPage == false)
            {
                foreach (var item in paginador.Datos)
                {
                    DetalleUpasActividades.Add(item);
                }
            }
        }
        private bool CanLoadMoreItems(object arg)
        {
            if (DetalleUpasActividades.Count >= TotalItems)
                return false;
            return true;
        }

        private async void LoadMoreItems(object obj)
        {
            try
            {
                IsBusy = true;
                await Task.Delay(1000);
                AddUpasConActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void AddUpasConActividades()
        {
            string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
            TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
            Response response = await _apiService.GetPaginadoAsyncWithToken<DetalleUpaActividadDTO>(url,
                $"UpasActividades/paginar?Page={Page}&&PageSize={PageSize}", UserApp.Token);
            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }
            Paginador<DetalleUpaActividadDTO> paginador = (Paginador<DetalleUpaActividadDTO>)response.Data;
            if (paginador.TotalPages <= 1)
            {
                return;
            }
            if (paginador.HasNextPage)
            {

                Page += 1;
                foreach (var item in paginador.Datos)
                {
                    DetalleUpasActividades.Add(item);
                }

            }
            if (DetalleUpasActividades.Count < TotalItems)
            {
                if (paginador.HasPreviousPage == true && paginador.HasNextPage == false)
                {
                    foreach (var item in paginador.Datos)
                    {
                        DetalleUpasActividades.Add(item);
                    }
                }
            }
        }
    }
}
