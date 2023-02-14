using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.App.Views.Sincronizaciones;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
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
        private bool _isOn;
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
            StateChangedCommand = new Command(SwitchStateChanged, CanNavigate);
            _fullName = TokenUser.User.FullName;
            _isOn = MovilSettings.IsOnNotificationsViaMail;
        }
        #endregion

        #region Properties
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }
        public bool IsOn
        {
            get => _isOn;
            set => SetProperty(ref _isOn, value);
        }
        #endregion

        #region Commands
        public Command EditProfileCommand { get; set; }
        public Command ChangePasswordCommand { get; set; }
        public Command HelpCommand { get; set; }
        public Command LogoutCommand { get; set; }
        public Command StateChangedCommand { get; set; }

        public Command MigrateDataToDeviceCommand
        {
            get
            {
                return new Command(async () =>
                {
                    var parameters = new NavigationParameters
                    {
                         { "TypeSyncronization", TipoSincronizacion.MigrateDataOnlineToDevice }
                    };
                    await _navigationService.NavigateAsync(nameof(SignOutPopupPage), parameters);
                });
            }
        }

        public Command MigrateDataToServerCommand
        {
            get
            {
                return new Command(async () =>
                {
                    var parameters = new NavigationParameters
                    {
                         { "TypeSyncronization", TipoSincronizacion.MigrateDataOfflineToServer }
                    };
                    await _navigationService.NavigateAsync(nameof(SignOutPopupPage), parameters);
                });
            }
        }

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
        private void SwitchStateChanged(object obj)
        {

            if (IsOn)
            {
                MovilSettings.IsOnNotificationsViaMail = true;
                ActivateNotificationsViaEmail();
            }
            else
            {
                MovilSettings.IsOnNotificationsViaMail = false;
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
        #endregion

    }
}
