using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels.Account
{
    /// <summary>
    /// ViewModel for forgot password page.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ForgotPasswordViewModel : LoginViewModel
    {

        #region Fields
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        #endregion

        #region Constructor
        public ForgotPasswordViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            this.SignUpCommand = new Command(this.SignUpClicked);
            this.SendCommand = new Command(this.SendClicked);
        }

        #endregion

        #region Command


        public Command SendCommand { get; set; }

        public Command SignUpCommand { get; set; }

        #endregion

        #region Methods
        private async void SendClicked(object obj)
        {
            if (this.IsEmailFieldValid())
            {
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        await PopupNavigation.Instance.PushAsync(new LoadingPopupPage());
                        ForgotPasswordRequest email = new ForgotPasswordRequest
                        {
                            Email = Email.Value,
                        };
                        Response respuesta = await _apiService.PutAsync(URL, "Account/forgotpassword", email);
                        if (!respuesta.IsExito)
                        {
                            AlertError(respuesta.Mensaje);
                            await ClosePopup();
                            return;
                        }
                        AlertSuccess(respuesta.Mensaje);
                        await _navigationService.NavigateAsync($"{nameof(ResetPasswordPage)}");
                        await ClosePopup();
                    }
                    else
                    {
                        AlertNoInternetConnection();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
        }

        private async void SignUpClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(RegistroPage));
        }

        #endregion
    }
}