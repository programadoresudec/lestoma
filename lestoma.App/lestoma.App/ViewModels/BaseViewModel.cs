using Android.Bluetooth;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels
{
    [Preserve(AllMembers = true)]
    [DataContract]
    public class BaseViewModel : BindableBase, IInitialize, INavigationAware, IDestructible, INotifyPropertyChanged
    {

        #region Fields

        private Command<object> backButtonCommand;
        private bool isBusy;
        public BluetoothAdapter mBluetoothAdapter { get; set; }
        protected INavigationService NavigationService { get; private set; }
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }

        private int _page = 1;
        public int Page
        {
            get { return _page; }
            set { SetProperty(ref _page, value); }
        }


        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }


        #endregion

        #region Event handler


        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            ActivarBluetoothCommand = new Command(OnBluetoothClicked);
        }

        #endregion

        #region Commands

        public Command<object> BackButtonCommand
        {
            get
            {
                return backButtonCommand ?? (backButtonCommand = new Command<object>(BackButtonClicked));
            }
        }
        public Command OnSignOutCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await NavigationService.NavigateAsync(nameof(SignOutPopupPage));
                });
            }
        }
        public Command ActivarBluetoothCommand { get; }

        private async void OnBluetoothClicked()
        {
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            //Verificamos que este habilitado
            if (!mBluetoothAdapter.Enable())
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth desactivado", "OK");
                return;
            }
            //verificamos que no sea nulo el sensor
            if (mBluetoothAdapter == null)
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth No Existe o esta Ocupado", "OK");
                return;
            }
        }
        #endregion

        #region Methods


        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.NotifyPropertyChanged(propertyName);

            return true;
        }


        private void BackButtonClicked(object obj)
        {
            if (Device.RuntimePlatform == Device.UWP && Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
            {
                Application.Current.MainPage.Navigation.PopAsync();
            }
            else if (Device.RuntimePlatform != Device.UWP && Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
            {
                Application.Current.MainPage.Navigation.PopAsync();
            }
            else if (Device.RuntimePlatform != Device.UWP && Application.Current.MainPage.Navigation.ModalStack.Count > 0)
            {
                Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        public virtual void Initialize(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {

        }

        public virtual void Destroy()
        {

        }
        #endregion

        #region Metodo parsear Data de un JSON OBJECT
        public T ParsearData<T>(Response respuesta)
        {
            JObject Jobject = JObject.FromObject(respuesta);
            JToken jToken = Jobject.GetValue("Data");
            return jToken.ToObject<T>();
        }


        public string URL => Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
        public TokenDTO TokenUser => !string.IsNullOrEmpty(MovilSettings.Token) ? JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token) : null;
        #endregion
    }
}
