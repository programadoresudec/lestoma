using Acr.UserDialogs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;

namespace lestoma.App.ViewModels.Buzon
{
    public class MoreInfoPopupViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private DetalleBuzonDTO _buzon;

        public MoreInfoPopupViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;

        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("BuzonId"))
            {
                var id = parameters.GetValue<int>("BuzonId");
                LoadData(id);
            }
        }
        public DetalleBuzonDTO Buzon
        {
            get => _buzon;
            set => SetProperty(ref _buzon, value);
        }
        private async void LoadData(int id)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (_apiService.CheckConnection())
                {
                    var response = await _apiService.GetAsyncWithToken(URL_API, $"buzon-de-reportes/info/{id}", TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertWarning(response.MensajeHttp);
                        return;
                    }
                    Buzon = ParsearData<DetalleBuzonDTO>(response);
                }
                else
                {
                    AlertNoInternetConnection();
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
    }
}
