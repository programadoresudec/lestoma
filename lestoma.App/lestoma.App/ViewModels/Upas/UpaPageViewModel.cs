using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class UpaPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<UpaRequest> _upas;
        
        
        public UpaPageViewModel(INavigationService navigationService, IApiService apiService): 
            base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            loadUpas();
        }

        private async void loadUpas()
        {
            try
            {
                await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                if (!_apiService.CheckConnection())
                {
                    CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                    return;
                }
                string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
                TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
                Response response = await _apiService.GetListAsyncWithToken<List<UpaRequest>>(url,
                    "Upas/listado", UserApp.Token);
                if (!response.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                    return;
                }
                Upas = new ObservableCollection<UpaRequest>((List<UpaRequest>)response.Data);
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

        public ObservableCollection<UpaRequest> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }

    }
}
