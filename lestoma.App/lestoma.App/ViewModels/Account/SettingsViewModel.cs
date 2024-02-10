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
        private readonly IApiService _apiService;
        private string _fullName;
        private bool _isOn;
        private bool _isNavigating = false;
        #region Constructor
        public SettingsViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
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
        public Command ChangePasswordCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!_isNavigating)
                    {
                        _isNavigating = true;
                        await NavigationService.NavigateAsync($"{nameof(ChangePasswordPage)}");
                        _isNavigating = false;
                    }

                });
            }
        }
        public Command LogoutCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!_isNavigating)
                    {
                        _isNavigating = true;
                        await NavigationService.NavigateAsync($"{nameof(SignOutPopupPage)}");
                        _isNavigating = false;
                    }
                });
            }
        }
        public Command StateChangedCommand { get; set; }

        public Command SetUpBluetoothCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!_isNavigating)
                    {
                        _isNavigating = true;
                        await NavigationService.NavigateAsync(nameof(MACBluetoothPopupPage));
                        _isNavigating = false;
                    }

                });
            }
        }

        public Command MigrateDataToDeviceCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!_isNavigating)
                    {
                        _isNavigating = true;
                        var parameters = new NavigationParameters
                            {
                                 { "TypeSyncronization", TipoSincronizacion.MigrateDataOnlineToDevice }
                            };
                        await NavigationService.NavigateAsync(nameof(SyncronizarDataPopupPage), parameters);
                        _isNavigating = false;
                    }
                });
            }
        }

        public Command MigrateDataToServerCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!_isNavigating)
                    {
                        _isNavigating = true;
                        var parameters = new NavigationParameters
                            {
                                 { "TypeSyncronization", TipoSincronizacion.MigrateDataOfflineToServer }
                            };
                        await NavigationService.NavigateAsync(nameof(SyncronizarDataPopupPage), parameters);
                        _isNavigating = false;
                    }
                });
            }
        }
        public Command RedirectionManualAppCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!_isNavigating)
                    {
                        _isNavigating = true;
                        await NavigationService.NavigateAsync(nameof(ManualPage));
                        _isNavigating = false;
                    }
                });
            }
        }

        #endregion

        #region Methods
        private bool CanNavigate(object arg)
        {
            return true;
        }
        private async void SwitchStateChanged(object obj)
        {
            if (IsOn)
            {

                var check = await UserDialogs.Instance.ConfirmAsync($"¿Está seguro de activar notificaciones via email?\n\n¡Tenga presente que si no activa las notificaciones no podrá recibir información.!",
                    "Alerta", "Aceptar", "Cancelar");
                if (check)
                {
                    MovilSettings.IsOnNotificationsViaMail = true;
                    ActivateNotificationsViaEmail();
                }
                else
                {
                    IsOn = !IsOn;
                }
            }
            else
            {
                var check = await UserDialogs.Instance.ConfirmAsync($"¿Está seguro de desactivar notificaciones via email?\n\n¡Tenga presente que no podrá recibir información, (reportes, correos entre otros)!",
                    "Alerta", "Aceptar", "Cancelar");
                if (check)
                {
                    MovilSettings.IsOnNotificationsViaMail = false;
                    DesactivateNotificationsViaEmail();
                }
                else
                {
                    IsOn = !IsOn;
                }
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
