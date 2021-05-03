using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using lestoma.App.Views;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels
{
    /// <summary>
    /// ViewModel for sign-up page.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class RegistroPageViewModel : LoginViewModel
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
        /// Initializes a new instance for the <see cref="RegistroPageViewModel" /> class.
        /// </summary>
        public RegistroPageViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService, apiService)
        {
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

        #region Methods

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
            this.Password.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password Required" });
            this.Password.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Re-enter Password" });
        }

        /// <summary>
        /// Invoked when the Log in button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private async void LoginClicked(object obj)
        {
            await _navigationService.NavigateAsync(nameof(LoginPage));
        }

        /// <summary>
        /// Invoked when the Sign Up button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private void SignUpClicked(object obj)
        {
            if (this.AreFieldsValid())
            {
                // Do something
            }
        }

        #endregion
    }
}