using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Responses;
using Plugin.Toast;
using Prism.Navigation;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels
{
    /// <summary>
    /// ViewModel for reset password page.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ResetPasswordViewModel : ViewModelBase
    {
        #region Fields

        private ValidatablePair<string> password;
        private ValidatableObject<string> verificationCode;
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;

        private bool _isRunning;
        private bool _isEnabled;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetPasswordViewModel" /> class.
        /// </summary>
        public ResetPasswordViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            this.InitializeProperties();
            this.AddValidationRules();
            _isEnabled = true;
            this.SubmitCommand = new Command(this.SubmitClicked, CanExecuteClickCommand);
            this.SignInCommand = new Command(this.SignInClicked, CanExecuteClickCommand);
        }
        #endregion

        #region Command

        /// <summary>
        /// Gets or sets the command that is executed when the Submit button is clicked.
        /// </summary>
        public Command SubmitCommand { get; set; }

        /// <summary>
        /// Gets or sets the command that is executed when the Sign Up button is clicked.
        /// </summary>
        public Command SignInCommand { get; set; }
        #endregion

        #region Public property

        /// <summary>
        /// Gets or sets the property that bounds with an entry that gets the new password from user in the reset password page.
        /// </summary>
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

        public ValidatableObject<string> VerificationCode
        {
            get
            {
                return this.verificationCode;
            }

            set
            {
                if (this.verificationCode == value)
                {
                    return;
                }

                this.SetProperty(ref this.verificationCode, value);
            }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                SignInCommand.ChangeCanExecute();
                SubmitCommand.ChangeCanExecute();

            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Initialize whether fieldsvalue are true or false.
        /// </summary>
        /// <returns>true or false </returns>
        public bool AreFieldsValid()
        {
            bool isPassword = this.Password.Validate();
            bool isVerificationCodeValid = this.VerificationCode.Validate();
            return isPassword && isVerificationCodeValid;
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
            this.VerificationCode = new ValidatableObject<string>();
            this.Password = new ValidatablePair<string>();
        }

        /// <summary>
        /// Validation rule for password
        /// </summary>
        private void AddValidationRules()
        {
            this.Password.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Contraseña requerida." });
            this.Password.Item1.Validations.Add(new IsLenghtValidRule<string> { ValidationMessage = "Debe tener entre 8 y 30 caracteres.", MaximumLenght = 30, MinimumLenght = 8 });
            this.Password.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "confirmar contraseña requerida." });
            this.Password.Validations.Add(new MatchPairValidationRule<string> { ValidationMessage = "Las contraseñas no coinciden." });
            this.VerificationCode.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Codigo requerido." });
            this.VerificationCode.Validations.Add(new IsLenghtValidRule<string> { ValidationMessage = "Debe tener 6 caracteres.", MaximumLenght = 6, MinimumLenght = 6 });
        }

        /// <summary>
        /// Invoked when the Submit button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
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
                RecoverPasswordRequest recover = new RecoverPasswordRequest()
                {
                    Codigo = this.VerificationCode.Value,
                    Password = this.Password.Item1.Value
                };
                Response respuesta = await _apiService.PostAsync(url, "Account/recoverpassword", recover);
                IsRunning = false;
                IsEnabled = true;
                if (!respuesta.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                    return;
                }
                CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                await Task.Delay(1000);
                await _navigationService.NavigateAsync(nameof(LoginPage));
            }
        }

        /// <summary>
        /// Invoked when the Sign Up button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private async void SignInClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(LoginPage));
        }

        #endregion
    }
}