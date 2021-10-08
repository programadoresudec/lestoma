using Android.Bluetooth;
using Java.IO;
using Java.Util;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class MandarTramaViewModel : BaseViewModel
    {
        private CancellationTokenSource _cancellationToken { get; set; }

        public string MessageToSend { get; set; }


        private readonly INavigationService _navigationService;
        private string _trama;
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;
        private static string address = "00:21:13:00:92:B8";
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        public MandarTramaViewModel(INavigationService navigationService) :
        base(navigationService)
        {
            _navigationService = navigationService;
            _cancellationToken = new CancellationTokenSource();
            ConnectionBluetoothCommand = new Command(ConectarBluetoothClicked);
            MandarRespuestaCommand = new Command(MandarRespuestaClicked);
        }

        private async void MandarRespuestaClicked(object obj)
        {
            await MandarTrama();
        }

        public string Trama
        {
            get => _trama;
            set => SetProperty(ref _trama, value);
        }

        public async void Connect()
        {
            //Iniciamos la conexion con el arduino
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            System.Console.WriteLine("Conexion en curso" + device);

            //Indicamos al adaptador que ya no sea visible
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                if (btSocket == null)
                {
                    btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                    //Conectamos el socket
                }
                //Inicamos el socket de comunicacion con el arduino
                await btSocket.ConnectAsync();
                if (btSocket.IsConnected)
                {
                    CrossToastPopUp.Current.ShowToastSuccess($"Conexión Establecida", Plugin.Toast.Abstractions.ToastLength.Long);
                }
            }
            catch (Exception ex)
            {
                //en caso de generarnos error cerramos el socket
                Debug.WriteLine(ex.Message);
                CrossToastPopUp.Current.ShowToastError($"Error: {ex.Message}");
                btSocket.Close();
            }
        }



        //Metodo de verificacion del sensor Bluetooth
        private async void CheckBt()
        {
            //asignamos el sensor bluetooth con el que vamos a trabajar
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
        private void ConectarBluetoothClicked()
        {
            CheckBt();
            Connect();
        }


        private async Task MandarTrama()
        {
            while (_cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    if (btSocket != null)
                    {
                        Debug.Write("Connected");
                        if (btSocket.IsConnected)
                        {
                            var mReader = new InputStreamReader(btSocket.InputStream);
                            var buffer = new BufferedReader(mReader);

                            while (_cancellationToken.IsCancellationRequested == false)
                            {
                                if (!string.IsNullOrWhiteSpace(Trama))
                                {
                                    var chars = Trama.ToCharArray();
                                    var bytes = new List<byte>();

                                    foreach (var character in chars)
                                    {
                                        bytes.Add((byte)character);
                                    }

                                    await btSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count);
                                    CrossToastPopUp.Current.ShowToastSuccess($"trama: {Trama} enviada");

                                    Trama = string.Empty;
                                    _cancellationToken.Cancel();
                                }
                            }
                        }
                    }
                    _cancellationToken.Cancel();
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                    Debug.Write(ex.Message);
                }
            }
            _cancellationToken = new CancellationTokenSource();
        }
        public Command MandarRespuestaCommand { get; set; }
        public Command ConnectionBluetoothCommand { get; set; }

    }
}
