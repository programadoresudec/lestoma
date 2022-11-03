using Acr.UserDialogs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Threading.Tasks;

namespace lestoma.App.ViewModels
{
    public class DashboardHangfireViewModel : BaseViewModel
    {
        private string _pathHangFire;
        private readonly IApiService _apiService;
        public DashboardHangfireViewModel(INavigationService navigationService, IApiService apiService)
             : base(navigationService)
        {
            _apiService = apiService;
            Title = "Dashboard Hang-Fire";
            LoadPath();
        }

        private async void LoadPath()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando");
                if (_apiService.CheckConnection())
                {
                    if (TokenUser.Expiration >= DateTime.Now)
                    {
                        PathHangFire = $"{URL_DOMINIO}dashboard-lestoma?jwt_token={TokenUser.Token}";
                    }
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
                await Task.Delay(1000);
                UserDialogs.Instance.HideLoading();
            }

        }

        public string PathHangFire
        {
            get => _pathHangFire;
            set => SetProperty(ref _pathHangFire, value);
        }
    }
}
