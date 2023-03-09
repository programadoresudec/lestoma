using Acr.UserDialogs;
using Android.Bluetooth;
using Java.Util;
using lestoma.App.Views;
using lestoma.App.Views.Laboratorio;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Plugin.Toast;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class ModulosUpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<NameDTO> _modulos;
        private bool _isCheckConnection;
        private BluetoothSocket btSocket = null;
        private string address;
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        public ModulosUpaViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
            Title = "Seleccione un modulo";
            _modulos = new ObservableCollection<NameDTO>();
            SeeComponentCommand = new Command<object>(ModuloSelected, CanNavigate);
            LoadModulos();
            address = MovilSettings.MacBluetooth;
            ConnectionBluetoothCommand = new Command(ConectarBluetoothClicked);
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }

        public ObservableCollection<NameDTO> Modulos
        {
            get => _modulos;
            set => SetProperty(ref _modulos, value);
        }

        public bool IsCheckConnection
        {
            get => _isCheckConnection;
            set => SetProperty(ref _isCheckConnection, value);
        }
        public Command ConnectionBluetoothCommand { get; set; }
        public Command SeeComponentCommand { get; set; }

        private async void ModuloSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var modulo = lista.ItemData as NameDTO;

            if (modulo == null)
                return;

            var parameters = new NavigationParameters
            {
                { "ModuloId", modulo.Id }
            };
            await _navigationService.NavigateAsync(nameof(ComponentesModuloPage), parameters);

        }
        private void LoadModulos()
        {
            IsCheckConnection = _apiService.CheckConnection();
            if (IsCheckConnection)
            {
                ConsumoService();
            }
            else
            {
                ConsumoServiceLocal();
            }
        }

        private async void ConectarBluetoothClicked(object obj)
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
                if (string.IsNullOrWhiteSpace(address)) {
                    AlertError("No hay conexión de MAC a ningún Bluetooth.");
                    return;
                }
                UserDialogs.Instance.ShowLoading("Conectando...");
                BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
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

        private void ConsumoServiceLocal()
        {
            throw new NotImplementedException();
        }


        private async void ConsumoService()
        {
            try
            {
                IsBusy = true;
                Modulos = new ObservableCollection<NameDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                    $"laboratorio-lestoma/listar-modulos-upa-actividad-por-usuario", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<NameDTO>)response.Data;
                    if (listado.Count > 0)
                    {
                        Modulos = new ObservableCollection<NameDTO>(listado);
                    }
                }
                else
                {
                    AlertWarning(response.MensajeHttp);
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                IsBusy = false;
            }

        }
    }
}

