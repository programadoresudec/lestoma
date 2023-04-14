using lestoma.App.Views.Account;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
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
        private readonly IApiService _apiService;
        #endregion

        #region Constructor
        public ForgotPasswordViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
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
                    IsBusy = true;
                    if (!_apiService.CheckConnection())
                    {
                        AlertNoInternetConnection();
                        return;
                    }
                    ForgotPasswordRequest email = new ForgotPasswordRequest
                    {
                        Email = Email.Value,
                    };
                    ResponseDTO respuesta = await _apiService.PutAsync(URL_API, "Account/forgotpassword", email);
                    if (!respuesta.IsExito)
                    {
                        AlertError(respuesta.MensajeHttp);
                        return;
                    }
                    AlertSuccess(respuesta.MensajeHttp);
                    await _navigationService.NavigateAsync($"{nameof(ResetPasswordPage)}");
                }
                catch (Exception ex)
                {
                    SeeError(ex);
                }
                finally
                {
                    IsBusy = false;
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