using Acr.UserDialogs;
using lestoma.App.Views.Account;
using lestoma.App.Views.Sincronizaciones;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.DatabaseOffline.IConfiguration;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Account
{
    public class SignOutPopupViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        public SignOutPopupViewModel(INavigationService navigationService, IUnitOfWork unitOfWork)
            : base(navigationService)
        {
            _unitOfWork = unitOfWork;
        }
        public Command SignOutCommand => new Command(SignOutCommandExecuted);
        public Command CancelarCommand => new Command(CancelarCommandExecuted);

        private async void SignOutCommandExecuted()
        {
            var count = await _unitOfWork.Laboratorio.CountData();
            if (count > 0)
            {
                var check = await UserDialogs.Instance.ConfirmAsync($"Tiene pendiente {count} registros de tramas por sincronizar a la nube.", "Alerta", "Aceptar", "Cancelar");
                if (check)
                {
                    var parameters = new NavigationParameters
                    {
                         { "TypeSyncronization", TipoSincronizacion.MigrateDataOfflineToServer }
                    };
                    await NavigationService.NavigateAsync(nameof(SyncronizarDataPopupPage), parameters);
                }
            }
            else
            {
                MovilSettings.Token = null;
                MovilSettings.IsLogin = false;
                MovilSettings.IsOnSyncToDevice = false;
                await _unitOfWork.EnsureDeletedBD();
                await NavigationService.NavigateAsync($"/NavigationPage/{nameof(LoginPage)}");
            }
        }
        private async void CancelarCommandExecuted() =>
            await NavigationService.GoBackAsync();
    }
}
