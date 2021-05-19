﻿using lestoma.App.Views;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Responses;
using Plugin.Toast;
using Prism.Navigation;
using Syncfusion.XForms.PopupLayout;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels
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
        private bool _isRunning;
        private bool _isEnabled;
        SfPopupLayout popupLayout;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ForgotPasswordViewModel" /> class.
        /// </summary>
        public ForgotPasswordViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _isEnabled = true;
            this.SignUpCommand = new Command(this.SignUpClicked, CanExecuteClickCommand);
            this.SendCommand = new Command(this.SendClicked, CanExecuteClickCommand);
            popupLayout = new SfPopupLayout();
        }

        #endregion

        #region Command

        /// <summary>
        /// Gets or sets the command that is executed when the Send button is clicked.
        /// </summary>
        public Command SendCommand { get; set; }

        /// <summary>
        /// Gets or sets the command that is executed when the Sign Up button is clicked.
        /// </summary>
        public Command SignUpCommand { get; set; }

        #endregion

        #region Properties
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
                SignUpCommand.ChangeCanExecute();
                SendCommand.ChangeCanExecute();
            }
        }
        bool CanExecuteClickCommand(object arg)
        {
            return _isEnabled;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoked when the Send button is clicked.
        /// </summary>
        /// <param name="obj">The Object</param>
        private async void SendClicked(object obj)
        {
            if (this.IsEmailFieldValid())
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
                ForgotPasswordRequest email = new ForgotPasswordRequest
                {
                    Email = this.Email.Value,
                };
                Response respuesta = await _apiService.PostAsync(url, "Account/forgotpassword", email);
                IsRunning = false;
                IsEnabled = true;
                if (!respuesta.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                    return;
                }
                CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                await Task.Delay(1000);

                popupLayout.Show();

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

        #endregion
    }
}