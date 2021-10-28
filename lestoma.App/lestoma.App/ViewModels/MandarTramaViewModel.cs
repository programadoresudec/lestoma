using Android.Bluetooth;
using Java.IO;
using Java.Util;
using lestoma.App.Views;
using lestoma.CRC.Helpers;
using Plugin.Toast;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly INavigationService _navigationService;
        private string _trama;
        private string _tramaRecibida;
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
        public string TramaRecibida
        {
            get => _tramaRecibida;
            set => SetProperty(ref _tramaRecibida, value);
        }
        public async void Connect()
        {

            await PopupNavigation.Instance.PushAsync(new LoadingPopupPage("Conectando..."));
            try
            {
                btSocket = null;
                mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (!mBluetoothAdapter.IsEnabled)
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
                    CrossToastPopUp.Current.ShowToastSuccess("Conexión Establecida.", Plugin.Toast.Abstractions.ToastLength.Long);
                }
            }
            catch (Exception ex)
            {

                //en caso de generarnos error cerramos el socket
                Debug.WriteLine(ex.Message);
                CrossToastPopUp.Current.ShowToastError($"Error: {ex.Message}");
                btSocket.Close();
            }
            finally
            {
                await _navigationService.ClearPopupStackAsync();
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

                            var bytes = new List<byte>();
                            byte[] bytesMOdbus = CalcularCRCHelper.CalculateCrc16Modbus(Trama);

                            var resultado = new List<byte>();
                            resultado.Add(bytesMOdbus.ElementAt(1));
                            resultado.Add(bytesMOdbus.ElementAt(0));

                            string hexa = HexaToByteHelper.ByteArrayToHexString(resultado.ToArray());

                            for (int i = 1; i < hexa.Length + 1; i++)
                            {
                                if (i % 2 == 0)
                                {
                                    hexa = hexa.Insert(i, " ");
                                    break;
                                }
                            }

                            Trama = Trama.Insert(Trama.Length, $" {hexa}");



                            string[] listaTramas = Trama.Split(" ");
                            // ajustar en 2 bytes cada caracter
                            System.Text.ASCIIEncoding codificador = new System.Text.ASCIIEncoding();
               
                            foreach (var item in listaTramas)
                            {
                                var letra = HexaToByteHelper.StringToByteArray(item);
                                bytes.Add(letra.ElementAt(0));
                            }

                            await btSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count, token);


                            CrossToastPopUp.Current.ShowToastSuccess($"trama: {Trama} enviada");
                            Trama = string.Empty;

                            byte[] bufferRecibido = new byte[10];  // buffer store for the stream     
                            int recibido = 0; // bytes returned from read()

                            recibido += await btSocket.InputStream.ReadAsync(bufferRecibido);
                            if (recibido > 0)
                            {
                                TramaRecibida = HexaToByteHelper.ByteArrayToHexString(bufferRecibido);
                            }
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
