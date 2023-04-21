using Acr.UserDialogs;
using Android.Bluetooth;
using Java.Util;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
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
        private static readonly UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Command<object> backButtonCommand;
        private bool isBusy;
        private int _pageSize;
        private int _pageNumber;
        private bool _isRefreshing;
        public BluetoothAdapter MBluetoothAdapter { get; set; }
        public static BluetoothSocket btSocket = null;

        protected INavigationService _navigationService { get; private set; }
        private string _title;
        private string _messageHelp;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        public string MessageHelp
        {
            get => _messageHelp;
            set => SetProperty(ref _messageHelp, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        public int Page
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public int TotalItems { get; set; }

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
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
            HelpCommand = new Command(ShowHelpClicked);
            Address = MovilSettings.MacBluetooth;
            _pageNumber = 1;
            _pageSize = 2;
        }
        #endregion

        #region Commands
        public Command ConnectionBluetoothCommand { get; }

        public Command HelpCommand { get; }
        public Command<object> BackButtonCommand
        {
            get
            {
                return backButtonCommand ??= new Command<object>(BackButtonClicked);
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

            MBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            //Verificamos que este habilitado
            if (!MBluetoothAdapter.Enable())
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth desactivado.", "Aceptar");
                return;
            }
            if (MBluetoothAdapter == null)
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth No Existe o está ocupado.", "Aceptar");
                return;
            }
        }

        protected async void ConectarBluetoothClicked(object obj)
        {
            try
            {
                MBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (!MBluetoothAdapter.IsEnabled)
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
                BluetoothDevice device = MBluetoothAdapter.GetRemoteDevice(Address);
                //Indicamos al adaptador que ya no sea visible
                MBluetoothAdapter.CancelDiscovery();
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
                btSocket.Close();
                SeeError(ex);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
        protected async void ShowHelpClicked()
        {
            await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: MessageHelp));
        }
        protected async void ClosePopup()
        {
            await _navigationService.ClearPopupStackAsync();
        }
        protected static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        protected async void SeeError(Exception exception, string errorMessage = "")
        {
            LestomaLog.Error($"{errorMessage} {exception.Message}");
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

        #region Alerts de CrossToast
        protected void AlertNoInternetConnection()
        {
            ToastConfig toasconfig = new ToastConfig("No tiene internet por favor active el wifi.")
            {
                Duration = TimeSpan.FromSeconds(3),
                Icon = "icon_nowifi.png",
                MessageTextColor = Color.White,
                BackgroundColor = Color.FromHex("#EFB459")
            };
            UserDialogs.Instance.Toast(toasconfig);
        }
        protected void AlertError(string Error = "", double seconds = 2.5, ToastPosition position = ToastPosition.Bottom)
        {
            ToastConfig toasconfig = new ToastConfig($"{Error}")
            {
                Position = position,
                Duration = TimeSpan.FromSeconds(seconds),
                MessageTextColor = Color.White,
                Icon = "icon_error.png",
                BackgroundColor = Color.FromHex("#E5502B")
            };
            UserDialogs.Instance.Toast(toasconfig);

        }
        protected void AlertWarning(string mensaje = "", double seconds = 2.5, ToastPosition position = ToastPosition.Bottom)
        {
            ToastConfig toasconfig = new ToastConfig($"{mensaje}")
            {
                Position = position,
                Duration = TimeSpan.FromSeconds(seconds),
                MessageTextColor = Color.White,
                Icon = "icon_warn.png",
                BackgroundColor = Color.FromHex("#ECAA00"),
            };
            UserDialogs.Instance.Toast(toasconfig);
        }

        protected void AlertSuccess(string mensaje = "EXITO", double seconds = 2.5, ToastPosition position = ToastPosition.Bottom)
        {
            ToastConfig toasconfig = new ToastConfig($"{mensaje}")
            {
                Position = position,
                Duration = TimeSpan.FromSeconds(seconds),
                MessageTextColor = Color.White,
                Icon = "icon_check.png",
                BackgroundColor = Color.FromHex("#79A300")
            };
            UserDialogs.Instance.Toast(toasconfig);
        }
        #endregion
    }
}
