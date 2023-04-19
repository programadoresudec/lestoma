using lestoma.App.Views.Account;
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
            MovilSettings.Token = null;
            MovilSettings.IsLogin = false;
            MovilSettings.IsOnSyncToDevice = false;
            await _unitOfWork.EnsureDeletedBD();
            await _navigationService.NavigateAsync($"/NavigationPage/{nameof(LoginPage)}");
        }
        private async void CancelarCommandExecuted() =>
            await _navigationService.GoBackAsync();
    }
}
