using Acr.UserDialogs;
using Android.Bluetooth;
using Java.Util;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.MyException;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
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
        private Command<object> backButtonCommand;
        private bool isBusy;
        private int _pageSize;
        private int _pageNumber;
        private bool _isRefreshing;
        public static BluetoothAdapter MBluetoothAdapter { get; set; }
        public static BluetoothSocket btSocket = null;

        protected INavigationService NavigationService { get; private set; }
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
            NavigationService = navigationService;
            ActivarBluetoothCommand = new Command(OnBluetoothClicked);
            ConnectionBluetoothCommand = new Command(ConectarBluetoothClicked);
            HelpCommand = new Command(ShowHelpClicked);
            Address = MovilSettings.MacBluetooth;
            _pageNumber = 1;
            _pageSize = 10;
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
                    await NavigationService.NavigateAsync(nameof(SignOutPopupPage));
                });
            }
        }
        public Command ActivarBluetoothCommand { get; }


        #endregion

        #region Methods

        protected async void OnBluetoothClicked()
        {
            if (MBluetoothAdapter != null)
            {
                if (MBluetoothAdapter.IsEnabled)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        AlertSuccess("El bluetooth ya se encuentra encendido.");
                    });
                    return;
                }
            }
            var confirmConfig = new ConfirmConfig
            {
                Title = "Alerta",
                Message = "¿Está seguro de encender el Bluetooth?",
                OkText = "Aceptar",
                CancelText = "Cancelar"
            };

            var result = await UserDialogs.Instance.ConfirmAsync(confirmConfig);

            if (result)
            {
                MBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                // Verificamos que esté habilitado
                if (!MBluetoothAdapter.Enable())
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Alerta", "Bluetooth desactivado.", "Aceptar");
                    });
                }

                if (MBluetoothAdapter == null)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Alerta", "Bluetooth no existe o está ocupado.", "Aceptar");
                    });
                }

                if (MBluetoothAdapter.Enable())
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Alerta", "El Bluetooth se ha encendido.", "Aceptar");
                    });
                }
            }
        }
        protected async Task<bool> ConnectUsingProfiles(BluetoothDevice device)
        {
            BluetoothProfileData bluetoothProfile = new BluetoothProfileData();
            bool isConnected = false;

            foreach (var profileEntry in bluetoothProfile.profileData)
            {
                var profileKey = profileEntry.Key;
                var profileInfo = profileEntry.Value;

                btSocket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(profileInfo.UUID));

                try
                {
                    await btSocket.ConnectAsync();
                    if (btSocket.IsConnected)
                    {
                        isConnected = true;
                        Console.WriteLine($"Conexión establecida con el perfil: {profileKey}");
                        break;
                    }
                }
                catch (Exception)
                {
                    // La conexión no se pudo establecer con este perfil, continúa con el siguiente
                    continue;
                }
            }

            return isConnected;
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
                    bool isConnected = await ConnectUsingProfiles(device);
                    if (!isConnected)
                    {
                        AlertError("No se pudo establecer conexión con ninguno de los perfiles disponibles.");
                        return;
                    }
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage("Conexión establecida."));
                }
                else
                {
                    //Inicamos el socket de comunicacion con el arduino
                    btSocket?.Close();
                    bool isConnected = await ConnectUsingProfiles(device);
                    if (!isConnected)
                    {
                        AlertError("No se pudo establecer conexión con ninguno de los perfiles disponibles.");
                        return;
                    }
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage("Conexión establecida."));
                }
            }
            catch (Exception ex)
            {
                //en caso de generarnos error cerramos el socket
                btSocket?.Close();
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
            await NavigationService.ClearPopupStackAsync();
        }
        protected static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Utiliza el servicio de ipify.org para obtener la dirección IP pública
                    var response = await client.GetStringAsync("https://api.ipify.org");
                    return response.Trim();
                }
            }
            catch
            {
                // En caso de error, regresa una cadena vacía
                return string.Empty;
            }
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
                ExceptionDictionary dictionary = new ExceptionDictionary();
                int exceptionCode = exception.HResult;
                if (exception.Message.Contains("socket"))
                {
                    var exceptionDescription = dictionary.GetExceptionDescription(exceptionCode);
                    if (exceptionDescription.IsExceptionCode)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage($"{exceptionDescription.MessageError}", Constants.ICON_ERROR));
                        await Task.Delay(2000);
                    }
                }
                else
                {
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage($"Error: {exception.Message}", Constants.ICON_ERROR));
                    await Task.Delay(2000);
                }

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
