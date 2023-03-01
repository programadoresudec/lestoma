﻿using lestoma.App.Views.Account;
using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Account
{
    public class SignOutPopupViewModel : BaseViewModel
    {
        public SignOutPopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {

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
