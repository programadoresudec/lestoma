using Acr.UserDialogs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;

namespace lestoma.App.ViewModels
{
    public class DashboardHangfireViewModel : BaseViewModel
    {
        private string _pathHangFire;
        private const string DOMINIO = "https://www.lestoma.site/";
        private readonly IApiService _apiService;
        public DashboardHangfireViewModel(INavigationService navigationService, IApiService apiService)
             : base(navigationService)
        {
            _apiService = apiService;
            Title = "Dashboard Hang-Fire";
            LoadPath();
        }

        private void LoadPath()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando", MaskType.Gradient);
                if (_apiService.CheckConnection())
                {
                    if (TokenUser.Expiration >= DateTime.Now)
                    {
                        PathHangFire = $"{DOMINIO}dashboard-lestoma?jwt_token={TokenUser.Token}";
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
