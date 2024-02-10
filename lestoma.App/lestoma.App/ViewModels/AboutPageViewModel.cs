using Acr.UserDialogs;
using lestoma.App.Views.Sincronizaciones;
using lestoma.CommonUtils.Enums;
using lestoma.DatabaseOffline.IConfiguration;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace lestoma.App.ViewModels
{
    public class AboutPageViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private bool _isNavigating = false;
        public AboutPageViewModel(INavigationService navigationService, IUnitOfWork unitOfWork)
             : base(navigationService)
        {
            _unitOfWork = unitOfWork;
            Title = "Acerca de LESTOMA";
            MessageHelp = "Si desea usar el apartado del laboratorio en modo offline, debe sincronizar los datos desde el menú de configuración\n\n ¡Sincronizar datos al modo offline!";
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("isModeOnline"))
            {
                var isModeOnline = parameters.GetValue<bool>("isModeOnline");
                if (isModeOnline)
                {
                    VerifySyncToCloud();
                }   
            }
        }
        private async void VerifySyncToCloud()
        {
            try
            {
                if (!_isNavigating)
                {
                    _isNavigating = true;
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        var count = await _unitOfWork.Laboratorio.CountData();
                        if (count > 0)
                        {
                            var check = await UserDialogs.Instance.ConfirmAsync($"Tiene pendiente {count} registros de tramas por sincronizar a la nube.", "Alerta", "Aceptar", "Cancelar");
                            if (check)
                            {
                                await Task.Delay(1000);
                                var parameters = new NavigationParameters
                                {
                                     { "TypeSyncronization", TipoSincronizacion.MigrateDataOfflineToServer }
                                };
                                await NavigationService.NavigateAsync(nameof(SyncronizarDataPopupPage), parameters);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                _isNavigating = false;
            }
        }
    }
}
