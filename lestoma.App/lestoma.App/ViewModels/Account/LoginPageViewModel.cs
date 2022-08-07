using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels.Account
{
    /// <summary>
    /// ViewModel for login with social icon page.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class LoginPageViewModel : LoginViewModel
    {
        #region Fields
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ValidatableObject<string> password;

        #endregion

        #region Constructor

        public LoginPageViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            Title = "Iniciar Sesión";
            _navigationService = navigationService;
            _apiService = apiService;
            InitializeProperties();
            AddValidationRules();
            LoginCommand = new Command(LoginClicked);
            SignUpCommand = new Command(SignUpClicked);
            ForgotPasswordCommand = new Command(ForgotPasswordClicked);
        }

        #endregion

        #region property

        public ValidatableObject<string> Password
        {
            get
            {
                return password;
            }

            set
            {
                if (password == value)
                {
                    return;
                }

                SetProperty(ref password, value);
            }
        }
        #endregion

        #region Command


        public Command LoginCommand { get; set; }


        public Command SignUpCommand { get; set; }


        public Command ForgotPasswordCommand { get; set; }
        #endregion

        #region validaciones
        public bool AreFieldsValid()
        {
            bool isEmailValid = Email.Validate();
            bool isPassword = Password.Validate();
            return isEmailValid && isPassword;
        }

        private void InitializeProperties()
        {
            Password = new ValidatableObject<string>();
        }
        private void AddValidationRules()
        {
            Password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Contraseña requerida." });
        }

        #endregion

        #region methods 
        private async void LoginClicked(object obj)
        {

            if (AreFieldsValid())
            {
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        await PopupNavigation.Instance.PushAsync(new LoadingPopupPage("Iniciando Sesión..."));
                        LoginRequest login = new LoginRequest
                        {
                            Email = Email.Value,
                            Clave = password.Value,
                            TipoAplicacion = (int)TipoAplicacion.AppMovil

                        };
                        Response respuesta = await _apiService.PostAsync(URL, "Account/login", login);
                        if (!respuesta.IsExito)
                        {
                            if (respuesta.StatusCode == (int)HttpStatusCode.Unauthorized)
                            {
                                AlertWarning(respuesta.Mensaje);
                            }
                            else
                            {
                                AlertError(respuesta.Mensaje);
                            }
                            await ClosePopup();
                            return;
                        }
                        TokenDTO token = ParsearData<TokenDTO>(respuesta);
                        MovilSettings.Token = JsonConvert.SerializeObject(token);
                        MovilSettings.IsLogin = true;
                        AlertSuccess(respuesta.Mensaje);
                        await _navigationService.NavigateAsync($"/{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                        await ClosePopup();
                    }
                    else
                    {
                        AlertNoInternetConnection();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    await ClosePopup();
                }
            }
        }
        private async void SignUpClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(RegistroPage));
        }


        private async void ForgotPasswordClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(ForgotPasswordPage));
        }
        #endregion
    }
}
