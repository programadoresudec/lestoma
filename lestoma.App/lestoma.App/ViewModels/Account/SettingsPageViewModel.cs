using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels.Account
{
    [Preserve(AllMembers = true)]
    public class SettingsPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        #region Constructor

        public SettingsPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            this.EditProfileCommand = new Command(this.EditProfileClicked);
            this.ChangePasswordCommand = new Command(this.ChangePasswordClicked);
            this.LinkAccountCommand = new Command(this.LinkAccountClicked);
            this.HelpCommand = new Command(this.HelpClicked);
            this.TermsCommand = new Command(this.TermsServiceClicked);
            this.PolicyCommand = new Command(this.PrivacyPolicyClicked);
            this.FAQCommand = new Command(this.FAQClicked);
            this.LogoutCommand = new Command(this.LogoutClicked);
        }
        #endregion

        #region Commands


        public Command EditProfileCommand { get; set; }


        public Command ChangePasswordCommand { get; set; }

        public Command LinkAccountCommand { get; set; }


        public Command HelpCommand { get; set; }


        public Command TermsCommand { get; set; }


        public Command PolicyCommand { get; set; }


        public Command FAQCommand { get; set; }


        public Command LogoutCommand { get; set; }

        #endregion

        #region Methods


        private void EditProfileClicked(object obj)
        {
            // Do something
        }


        private async void ChangePasswordClicked(object obj)
        {
            await _navigationService.NavigateAsync($"{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(ChangePasswordPage)}");
        }


        private void LinkAccountClicked(object obj)
        {
            // Do something
        }


        private void TermsServiceClicked(object obj)
        {
            // Do something
        }


        private void PrivacyPolicyClicked(object obj)
        {
            // Do something
        }


        private void FAQClicked(object obj)
        {
            // Do something
        }


        private void HelpClicked(object obj)
        {
            // Do something
        }


        private async void LogoutClicked(object obj)
        {
            await _navigationService.NavigateAsync($"{nameof(SignOutPopupPage)}");
        }

        #endregion
    }
}
