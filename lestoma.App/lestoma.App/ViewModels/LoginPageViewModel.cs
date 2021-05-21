using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Responses;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels
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
        private bool _isRunning;
        private bool _isEnabled;
        private ValidatableObject<string> password;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance for the <see cref="LoginPageViewModel" /> class.
        /// </summary>
        public LoginPageViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            Title = "Iniciar Sesión";
            _navigationService = navigationService;
            _apiService = apiService;
            this.InitializeProperties();
            this.AddValidationRules();
            _isEnabled = true;
            this.LoginCommand = new Command(this.LoginClicked, CanExecuteClickCommand);
            this.SignUpCommand = new Command(this.SignUpClicked, CanExecuteClickCommand);
            this.ForgotPasswordCommand = new Command(this.ForgotPasswordClicked, CanExecuteClickCommand);
        }

        #endregion

        #region property

        /// <summary>
        /// Gets or sets the property that is bound with an entry that gets the password from user in the login page.
        /// </summary>
        public ValidatableObject<string> Password
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
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                ForgotPasswordCommand.ChangeCanExecute();
                SignUpCommand.ChangeCanExecute();
                LoginCommand.ChangeCanExecute();

            }
        }
        #endregion

        #region Command

        /// <summary>
        /// Gets or sets the command that is executed when the Log In button is clicked.
        /// </summary>
        public Command LoginCommand { get; set; }

        /// <summary>
        /// Gets or sets the command that is executed when the Sign Up button is clicked.
        /// </summary>
        public Command SignUpCommand { get; set; }

        /// <summary>
        /// Gets or sets the command that is executed when the Forgot Password button is clicked.
        /// </summary>
        public Command ForgotPasswordCommand { get; set; }
        #endregion

        #region validaciones

        /// <summary>
        /// check the validation
        /// </summary>
        /// <returns>returns bool value</returns>
        public bool AreFieldsValid()
        {
            bool isEmailValid = this.Email.Validate();
            bool isPassword = this.Password.Validate();
            return isEmailValid && isPassword;
        }
        bool CanExecuteClickCommand(object arg)
        {
            return _isEnabled;
        }
        /// <summary>
        /// Initializing the properties.
        /// </summary>
        private void InitializeProperties()
        {
            this.Password = new ValidatableObject<string>();
        }

        /// <summary>
        /// Validation rules for password
        /// </summary>
        private void AddValidationRules()
        {
            this.Password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Contraseña requerida." });
        }

        #endregion

        #region methods

        /// <summary>
        /// Invoked when the Log In button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private async void LoginClicked(object obj)
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
                LoginRequest login = new LoginRequest
                {
                    Email = this.Email.Value,
                    Clave = this.password.Value
                };
                Response respuesta = await _apiService.PostAsync(url, "Account/login", login);
                IsRunning = false;
                IsEnabled = true;
                if (!respuesta.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                    return;
                }
                TokenResponse token = ParsearData<TokenResponse>(respuesta);
                MovilSettings.Token = JsonConvert.SerializeObject(token);
                MovilSettings.IsLogin = true;
                this.Password.CleanOnChange = false;
                this.password.Value = string.Empty;
                CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                await Task.Delay(1000);
                if (token.Rol.Equals(TipoRol.Administrador.ToString()))
                {
                   await _navigationService.NavigateAsync($"/{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                }
                else if (token.Rol.Equals(TipoRol.Auxiliar.ToString()))
                {
                
                   
                }
            }
        }



        /// <summary>
        /// Invoked when the Sign Up button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private async void SignUpClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(RegistroPage));
        }

        /// <summary>
        /// Invoked when the Forgot Password button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private async void ForgotPasswordClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(ForgotPasswordPage));
        }
        #endregion
    }
}
