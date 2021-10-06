using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class UpaPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<UpaRequest> _upas;


        public UpaPageViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _upas = new ObservableCollection<UpaRequest>();
            LoadUpas();
            LoadMoreItemsCommand = new Command<object>(LoadMoreItems, CanLoadMoreItems);
        }
        public ObservableCollection<UpaRequest> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }
        private async void LoadMoreItems(object obj)
        {

            try
            {
                IsBusy = true;
                await Task.Delay(1000);
                addUpas();
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

        private async void addUpas()
        {
            string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
            TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
            Response response = await _apiService.GetPaginadoAsyncWithToken<UpaRequest>(url,
                $"Upas/paginar?Page={Page}&&PageSize={PageSize}", UserApp.Token);
            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }
            Paginador<UpaRequest> paginador = (Paginador<UpaRequest>)response.Data;
            if (paginador.HasNextPage)
            {

                Page += 1;
                foreach (var item in paginador.Datos)
                {
                    Upas.Add(item);
                }

            }
            if (Upas.Count < TotalItems)
            {
                if (paginador.HasPreviousPage == true && paginador.HasNextPage == false)
                {
                    foreach (var item in paginador.Datos)
                    {
                        Upas.Add(item);
                    }
                }
            }
        }
        private bool CanLoadMoreItems(object obj)
        {

            if (Upas.Count >= TotalItems)
                return false;
            return true;
        }

        public Command<object> LoadMoreItemsCommand { get; set; }

        private async void LoadUpas()
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
            Response response = await _apiService.GetPaginadoAsyncWithToken<UpaRequest>(url,
                $"Upas/paginar?Page={Page}&&PageSize={PageSize}", UserApp.Token);
            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }
            Paginador<UpaRequest> paginador = (Paginador<UpaRequest>)response.Data;
            TotalItems = paginador.TotalDatos;
            if (paginador.HasNextPage)
            {
                Page += 1;
                if (Upas.Count > 0)
                {
                    foreach (var item in paginador.Datos)
                    {
                        Upas.Add(item);
                    }
                }
                else
                {
                    Upas = new ObservableCollection<UpaRequest>(paginador.Datos);
                }
            }


        }



    }
}
