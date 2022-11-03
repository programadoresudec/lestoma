﻿using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
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
                        ResponseDTO respuesta = await _apiService.PutAsync(URL_API, "Account/forgotpassword", email);
                        if (respuesta.IsExito)
                        {
                            AlertSuccess(respuesta.MensajeHttp);
                            await _navigationService.NavigateAsync($"{nameof(ResetPasswordPage)}"); ;
                        }
                        else
                        {
                            AlertError(respuesta.MensajeHttp);
                        }
                        ClosePopup();
                    }
                    else
                    {
                        AlertNoInternetConnection();
                    }
                }
                catch (Exception ex)
                {
                    if (PopupNavigation.Instance.PopupStack.Any())
                        await PopupNavigation.Instance.PopAsync();
                    SeeError(ex);
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