using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
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
    /// ViewModel for sign-up page.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class RegistroViewModel : LoginViewModel
    {
        #region Fields
        private ValidatableObject<string> name;
        private ValidatableObject<string> lastName;
        private ValidatablePair<string> password;
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance for the <see cref="RegistroViewModel" /> class.
        /// </summary>
        public RegistroViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            Title = "Registrarse";
            _navigationService = navigationService;
            _apiService = apiService;
            this.InitializeProperties();
            this.AddValidationRules();
            this.LoginCommand = new Command(this.LoginClicked);
            this.SignUpCommand = new Command(this.SignUpClicked);
        }
        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the property that bounds with an entry that gets the name from user in the Sign Up page.
        /// </summary>
        public ValidatableObject<string> Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.SetProperty(ref this.name, value);
            }
        }

        public ValidatableObject<string> LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (this.lastName == value)
                {
                    return;
                }

                this.SetProperty(ref this.lastName, value);
            }
        }

        /// <summary>
        /// Gets or sets the property that bounds with an entry that gets the password from users in the Sign Up page.
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
        #endregion

        #region validaciones

        /// <summary>
        /// Initialize whether fieldsvalue are true or false.
        /// </summary>
        /// <returns>true or false </returns>
        public bool AreFieldsValid()
        {
            bool isEmail = this.Email.Validate();
            bool isNameValid = this.Name.Validate();
            bool isLastNameValid = this.LastName.Validate();
            bool isPasswordValid = this.Password.Validate();

            return isPasswordValid && isNameValid && isLastNameValid && isEmail;
        }
        /// <summary>
        /// Initializing the properties.
        /// </summary>
        private void InitializeProperties()
        {
            this.Name = new ValidatableObject<string>();
            this.LastName = new ValidatableObject<string>();
            this.Password = new ValidatablePair<string>();
        }

        /// <summary>
        /// this method contains the validation rules
        /// </summary>
        private void AddValidationRules()
        {
            this.Name.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Nombre requerido." });
            this.LastName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Apellido requerido." });
            this.Password.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Contraseña requerida." });
            this.Password.Item1.Validations.Add(new IsLenghtValidRule<string> { ValidationMessage = "Debe tener entre 8 y 30 caracteres.", MaximumLenght = 30, MinimumLenght = 8 });
            this.Password.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "confirmar contraseña requerida." });
            this.Password.Validations.Add(new MatchPairValidationRule<string> { ValidationMessage = "Las contraseñas no coinciden." });
        }
        #endregion

        #region Methods

        private async void LoginClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(LoginPage));
        }

        private async void SignUpClicked(object obj)
        {
            try
            {
                if (AreFieldsValid())
                {
                    if (_apiService.CheckConnection())
                    {
                        await PopupNavigation.Instance.PushAsync(new LoadingPopupPage("Registrando..."));
                        UsuarioRequest usuario = new UsuarioRequest
                        {
                            Email = Email.Value,
                            Clave = Password.Item1.Value,
                            Apellido = LastName.Value,
                            Nombre = Name.Value
                        };
                        ResponseDTO respuesta = await _apiService.PostAsync(URL, "Account/registro", usuario);
                        if (respuesta.IsExito)
                        {
                            AlertSuccess(respuesta.MensajeHttp);
                            await _navigationService.GoBackAsync();
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
            }
            catch (Exception ex)
            {
                if (PopupNavigation.Instance.PopupStack.Any())
                    await PopupNavigation.Instance.PopAsync();
                SeeError(ex);
            }
        }

        #endregion
    }
}