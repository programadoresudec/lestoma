﻿using Acr.UserDialogs;
using Android.Bluetooth;
using Android.Locations;
using Java.Util;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Toast;
using Prism.Mvvm;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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
        private static string Address;
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Command<object> backButtonCommand;
        private bool isBusy;
        public BluetoothAdapter mBluetoothAdapter { get; set; }
        public static BluetoothSocket btSocket = null;

        protected INavigationService _navigationService { get; private set; }
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }
        public int Page { get; set; } = 1;


        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }
        #endregion

        #region Variables globales que quedan en persistencia
        protected string URL_API => Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();

        protected string URL_DOMINIO => Prism.PrismApplicationBase.Current.Resources["UrlDominio"].ToString();

        protected TokenDTO TokenUser => !string.IsNullOrEmpty(MovilSettings.Token) ? JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token) : null;

        protected int Rol => TokenUser != null ? TokenUser.User.RolId : 0;
        #endregion

        #region Event handler


        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public BaseViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ActivarBluetoothCommand = new Command(OnBluetoothClicked);
            ConnectionBluetoothCommand = new Command(ConectarBluetoothClicked);
            Address = MovilSettings.MacBluetooth;
        }
        #endregion

        #region Commands
        public Command ConnectionBluetoothCommand { get; set; }

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
                    await _navigationService.NavigateAsync(nameof(SignOutPopupPage));
                });
            }
        }
        public Command ActivarBluetoothCommand { get; }


        #endregion

        #region Methods
        protected async void OnBluetoothClicked()
        {

            #pragma warning disable CS0618 // Type or member is obsolete
                        mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            #pragma warning restore CS0618 // Type or member is obsolete
                        //Verificamos que este habilitado
            #pragma warning disable CS0618 // Type or member is obsolete
            if (!mBluetoothAdapter.Enable())
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth desactivado", "OK");
                return;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            //verificamos que no sea nulo el sensor
            if (mBluetoothAdapter == null)
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth No Existe o esta Ocupado", "OK");
                return;
            }
        }

        protected async void ConectarBluetoothClicked(object obj)
        {
            try
            {
                mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (!mBluetoothAdapter.IsEnabled)
                {
                    AlertError("Debe prender el bluetooth.");
                    return;
                }
                //Iniciamos la conexion con el arduino
                if (string.IsNullOrWhiteSpace(Address))
                {
                    AlertError("No hay conexión de MAC a ningún Bluetooth.");
                    return;
                }
                UserDialogs.Instance.ShowLoading("Conectando...");
                BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(Address);
                //Indicamos al adaptador que ya no sea visible
                mBluetoothAdapter.CancelDiscovery();
                if (btSocket == null)
                {
                    btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                    await btSocket.ConnectAsync();
                    if (btSocket.IsConnected)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage("Conexión establecida."));
                    }
                }
                else
                {
                    //Inicamos el socket de comunicacion con el arduino
                    btSocket.Close();
                    btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                    await btSocket.ConnectAsync();
                    if (btSocket.IsConnected)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage("Conexión establecida."));
                    }
                }
            }
            catch (Exception ex)
            {
                //en caso de generarnos error cerramos el socket
                LestomaLog.Error(ex.Message);
                btSocket.Close();
                SeeError(ex);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
        protected async void ClosePopup()
        {
            await _navigationService.ClearPopupStackAsync();
        }


        protected async void SeeError(Exception exception)
        {
            if (exception.Message.Contains("StatusCode"))
            {
                ResponseDTO error = JsonConvert.DeserializeObject<ResponseDTO>(exception.Message);
                await PopupNavigation.Instance.PushAsync(new MessagePopupPage($"Error {error.StatusCode}: {error.MensajeHttp}", Constants.ICON_ERROR));
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new MessagePopupPage($"Error: {exception.Message}", Constants.ICON_ERROR));
            }
        }

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
        public static object GetInternalProperty(Type type, object obj, string propertyName)
        {
            var property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(obj);
            }

            return null;
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
        public T ParsearData<T>(ResponseDTO respuesta)
        {
            JObject Jobject = JObject.FromObject(respuesta);
            JToken jToken = Jobject.GetValue("Data");
            return jToken.ToObject<T>();
        }

        #endregion

        #region Alerts de CrossToastPopUp
        protected void AlertNoInternetConnection()
        {
            CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.",
                Plugin.Toast.Abstractions.ToastLength.Long);

        }
        protected void AlertError(string Error = "")
        {
            CrossToastPopUp.Current.ShowToastError($"ERROR {Error}", Plugin.Toast.Abstractions.ToastLength.Long);

        }
        protected void AlertWarning(string mensaje = "")
        {
            CrossToastPopUp.Current.ShowToastWarning($"{mensaje}", Plugin.Toast.Abstractions.ToastLength.Long);

        }

        protected void AlertSuccess(string mensaje = "EXITO")
        {
            CrossToastPopUp.Current.ShowToastSuccess($"{mensaje}", Plugin.Toast.Abstractions.ToastLength.Long);
        }
        #endregion
    }
}
