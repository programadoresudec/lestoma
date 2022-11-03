using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Account
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        #region Fields
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private TokenDTO _userApp;
        private ValidatablePair<string> password;
        private ValidatableObject<string> currentPassword;
        #endregion

        #region Constructor

        public ChangePasswordViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            Title = "Iniciar Sesión";
            _navigationService = navigationService;
            _apiService = apiService;
            this.InitializeProperties();
            this.AddValidationRules();
            LoadUser();
            this.SubmitCommand = new Command(this.SubmitClicked);
        }
        #endregion

        #region Command

        public Command SubmitCommand { get; set; }

        #endregion

        #region Public property

        public ValidatablePair<string> Password
        {
            get
            {
                return this.password;
            }

            set
            {
                if (this.password == value)
                {
                    return;
                }

                this.SetProperty(ref this.password, value);
            }
        }

        public ValidatableObject<string> CurrentPassword
        {
            get
            {
                return this.currentPassword;
            }

            set
            {
                if (this.currentPassword == value)
                {
                    return;
                }

                this.SetProperty(ref this.currentPassword, value);
            }
        }
        public TokenDTO UserApp
        {
            get => _userApp;
            set => SetProperty(ref _userApp, value);
        }

        public void LoadUser()
        {
            if (MovilSettings.IsLogin)
            {
                this.UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
            }
        }
        #endregion

        #region validaciones

        public bool AreFieldsValid()
        {
            bool isPassword = this.Password.Validate();
            bool isCurrentPasswordValid = this.CurrentPassword.Validate();
            return isPassword && isCurrentPasswordValid;
        }

        private void InitializeProperties()
        {
            this.Password = new ValidatablePair<string>();
            this.CurrentPassword = new ValidatableObject<string>();
        }

        private void AddValidationRules()
        {
            this.Password.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Contraseña requerida." });
            this.Password.Item1.Validations.Add(new IsLenghtValidRule<string> { ValidationMessage = "Debe tener entre 8 y 30 caracteres.", MaximumLenght = 30, MinimumLenght = 8 });
            this.Password.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "confirmar contraseña requerida." });
            this.Password.Validations.Add(new MatchPairValidationRule<string> { ValidationMessage = "Las contraseñas no coinciden." });
            this.CurrentPassword.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "contraseña actual requerida." });
        }
        #endregion

        #region methods

        private async void SubmitClicked(object obj)
        {
            if (this.AreFieldsValid())
            {
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        await PopupNavigation.Instance.PushAsync(new LoadingPopupPage());
                        ChangePasswordRequest cambio = new ChangePasswordRequest
                        {
                            IdUser = UserApp.User.Id,
                            OldPassword = this.CurrentPassword.Value,
                            NewPassword = this.Password.Item1.Value
                        };
                        ResponseDTO respuesta = await _apiService.PostAsyncWithToken(URL_API, "Account/changepassword", cambio, UserApp.Token);
                        if (respuesta.IsExito)
                        {
                            AlertSuccess(respuesta.MensajeHttp);
                            await _navigationService.NavigateAsync($"/{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(SettingsPage)}");
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
                    await PopupNavigation.Instance.PopAsync();
                    SeeError(ex);
                }
            }
        }
        #endregion
    }
}
