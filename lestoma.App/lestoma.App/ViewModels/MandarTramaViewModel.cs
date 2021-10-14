using Android.Bluetooth;
using Java.IO;
using Java.Util;
using lestoma.CRC.Helpers;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class MandarTramaViewModel : BaseViewModel
    {
        CancellationTokenSource tcs = new CancellationTokenSource();
        CancellationToken token = new CancellationToken();

        public string MessageToSend { get; set; }


        private readonly INavigationService _navigationService;
        private string _trama;
        private BluetoothSocket btSocket = null;
        private static string address = "00:21:13:00:92:B8";
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        public MandarTramaViewModel(INavigationService navigationService) :
        base(navigationService)
        {
            _navigationService = navigationService;
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
            try
            {
                if (mBluetoothAdapter == null)
                {
                    CrossToastPopUp.Current.ShowToastError("Tiene que prender el bluetooth", Plugin.Toast.Abstractions.ToastLength.Long);
                    return;
                }
                //Iniciamos la conexion con el arduino
                BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
                System.Console.WriteLine("Conexion en curso" + device);

                //Indicamos al adaptador que ya no sea visible
                mBluetoothAdapter.CancelDiscovery();

                if (btSocket == null)
                {
                    btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                    //Conectamos el socket
                }
                //Inicamos el socket de comunicacion con el arduino
                await btSocket.ConnectAsync();
                if (btSocket.IsConnected)
                {
                    CrossToastPopUp.Current.ShowToastSuccess("Conexión Establecida", Plugin.Toast.Abstractions.ToastLength.Long);
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

        private void ConectarBluetoothClicked()
        {
            Connect();
        }


        private async Task MandarTrama()
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

                        if (!string.IsNullOrWhiteSpace(Trama))
                        {
                            var chars = Trama.ToCharArray();
                            var bytes = new List<byte>();

                            foreach (var character in chars)
                            {
                                bytes.Add((byte)character);
                            }
                            await btSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count, token);
                            CrossToastPopUp.Current.ShowToastSuccess($"trama: {Trama} enviada");
                            byte[] bytesMOdbus = CalcularCRCHelper.CalculateCrc16Modbus(Trama);
                            List<string> bytesString = new List<string>();

                            for (int i = 0; i < bytesMOdbus.Length; i++)
                            {
                                bytesString.Add(bytesMOdbus[i].ToString());
                                if (i == 1)
                                {
                                    bytesString.Insert(0, bytesMOdbus[i].ToString());
                                }
                            }
                            await Application.Current.MainPage.DisplayAlert("Modbus", $"{bytesString.ElementAt(0)} - {bytesString.ElementAt(1)}", "OK");
                            Trama = string.Empty;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.Write(ex);
                Debug.Write(ex.Message);
            }
            finally
            {
                tcs.Cancel();
            }
        }
        public Command MandarRespuestaCommand { get; set; }
        public Command ConnectionBluetoothCommand { get; set; }

    }
}
