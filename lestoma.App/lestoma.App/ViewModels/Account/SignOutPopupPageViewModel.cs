using lestoma.App.Views.Account;
using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Account
{
    public class SignOutPopupPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        public SignOutPopupPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
        }
        public Command SignOutCommand => new Command(SignOutCommandExecuted);
        public Command CancelarCommand => new Command(CancelarCommandExecuted);

        private async void SignOutCommandExecuted()
        {
            MovilSettings.Token = null;
            MovilSettings.IsLogin = false;
            await _navigationService.NavigateAsync($"/NavigationPage/{nameof(LoginPage)}");
        }
        private async void CancelarCommandExecuted() =>
            await _navigationService.GoBackAsync();
    }
}
