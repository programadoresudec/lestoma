﻿using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        #region Fields
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private bool _isRunning;
        private bool _isEnabled;
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
            _isEnabled = true;
            this.InitializeProperties();
            this.AddValidationRules();
            LoadUser();
            this.SubmitCommand = new Command(this.SubmitClicked, CanExecuteClickCommand);
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

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                SubmitCommand.ChangeCanExecute();

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
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }
        #endregion

        #region validaciones

        public bool AreFieldsValid()
        {
            bool isPassword = this.Password.Validate();
            bool isCurrentPasswordValid = this.CurrentPassword.Validate();
            return isPassword && isCurrentPasswordValid;
        }
        bool CanExecuteClickCommand(object arg)
        {
            return _isEnabled;
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
                IsRunning = true;
                IsEnabled = false;
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    IsRunning = false;
                    IsEnabled = true;
                    CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                    return;
                }

                string url = App.Current.Resources["UrlAPI"].ToString();
                ChangePasswordRequest cambio = new ChangePasswordRequest
                {
                    IdUser = UserApp.User.Id,
                    OldPassword = this.CurrentPassword.Value,
                    NewPassword = this.Password.Item1.Value
                };
                Response respuesta = await _apiService.PostAsyncWithToken(url, "Account/changepassword", cambio, UserApp.Token, MovilSettings.IsLogin);
                IsRunning = false;
                IsEnabled = true;
                if (!respuesta.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                    return;
                }
                CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                await Task.Delay(2000);
                await _navigationService.NavigateAsync($"/{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(SettingsPage)}");
            }
        }
        #endregion
    }
}
