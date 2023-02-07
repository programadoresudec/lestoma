using lestoma.App.Views.Account;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels.Account
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private string _fullName;
        #region Constructor

        public SettingsViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            this.EditProfileCommand = new Command(this.EditProfileClicked);
            this.ChangePasswordCommand = new Command(this.ChangePasswordClicked);
            this.HelpCommand = new Command(this.HelpClicked);
            this.LogoutCommand = new Command(this.LogoutClicked);
            _fullName = TokenUser.User.FullName;
        }
        #endregion

        #region Properties
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }
        #endregion

        #region Commands
        public Command EditProfileCommand { get; set; }
        public Command ChangePasswordCommand { get; set; }
        public Command HelpCommand { get; set; }
        public Command LogoutCommand { get; set; }
        #endregion

        #region Methods
        private void EditProfileClicked(object obj)
        {
            // Do something
        }

        private async void ChangePasswordClicked(object obj)
        {
            await _navigationService.NavigateAsync($"{nameof(ChangePasswordPage)}");
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
