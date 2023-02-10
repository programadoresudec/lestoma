using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels.Account
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private string _fullName;
        #region Constructor

        public SettingsViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            EditProfileCommand = new Command(EditProfileClicked);
            ChangePasswordCommand = new Command(ChangePasswordClicked);
            HelpCommand = new Command(HelpClicked);
            LogoutCommand = new Command(LogoutClicked);
            StateChangedCommand = new Command<object>(SwitchStateChanged, CanNavigate);
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
        public Command StateChangedCommand { get; set; }
        #endregion

        #region Methods
        private bool CanNavigate(object arg)
        {
            return true;
        }
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
        private void SwitchStateChanged(object obj)
        {
            var SwitchState = obj as Syncfusion.XForms.Buttons.SwitchStateChangedEventArgs;
            if (SwitchState.NewValue.Value)
            {
                ActivateNotificationsViaEmail();
            }
            else
            {
                DesactivateNotificationsViaEmail();

            }
        }
        private async void ActivateNotificationsViaEmail()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Activando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.PostWithoutBodyAsyncWithToken(URL_API,
                   $"Account/enable-notifications-by-mail", TokenUser.Token);
                    if (response.IsExito)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(response.MensajeHttp));
                    }
                    else
                    {
                        AlertWarning(response.MensajeHttp);
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
        private async void DesactivateNotificationsViaEmail()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Desactivando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.PostWithoutBodyAsyncWithToken(URL_API,
                   $"Account/disable-notifications-by-mail", TokenUser.Token);
                    if (response.IsExito)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(response.MensajeHttp));
                    }
                    else
                    {
                        AlertWarning(response.MensajeHttp);
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
    }
}
