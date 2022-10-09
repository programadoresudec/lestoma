using Android.Bluetooth;
using Java.Util;
using lestoma.App.Views;
using lestoma.CommonUtils.Helpers;
using Plugin.Toast;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
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
        private string _trama;
        private string _tramaRecibida;
        private BluetoothSocket btSocket = null;
        private static string address = "00:21:13:00:92:B8";
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        [Obsolete]
        public MandarTramaViewModel(INavigationService navigationService) :
        base(navigationService)
        {
            ConnectionBluetoothCommand = new Command(ConectarBluetoothClicked);
            MandarRespuestaCommand = new Command(MandarRespuestaClicked);

        }
        public Command MandarRespuestaCommand { get; set; }
        public Command RecibirRespuestaCommand { get; set; }
        public Command ConnectionBluetoothCommand { get; set; }
        private async void MandarRespuestaClicked()
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

        [Obsolete]
        public async void Connect()
        {

            await PopupNavigation.Instance.PushAsync(new LoadingPopupPage("Conectando..."));
            try
            {
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
                    await btSocket.ConnectAsync();
                    if (btSocket.IsConnected)
                    {
                        CrossToastPopUp.Current.ShowToastSuccess("Conexión Establecida.", Plugin.Toast.Abstractions.ToastLength.Long);
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
                        CrossToastPopUp.Current.ShowToastSuccess("Conexión Establecida.", Plugin.Toast.Abstractions.ToastLength.Long);
                    }
                }
            }
            catch (Exception ex)
            {

                //en caso de generarnos error cerramos el socket
                LestomaLog.Error(ex.Message);
                CrossToastPopUp.Current.ShowToastError($"Error: {ex.Message}");
                btSocket.Close();
            }
            finally
            {
                await _navigationService.ClearPopupStackAsync();
            }
        }

        [Obsolete]
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

                        if (!string.IsNullOrWhiteSpace(Trama))
                        {

                            var bytes = new List<byte>();
                            byte[] bytesMOdbus = new CRCHelper().CalculateCrc16Modbus(Trama);

                            var resultado = new List<byte>();
                            resultado.Add(bytesMOdbus.ElementAt(1));
                            resultado.Add(bytesMOdbus.ElementAt(0));

                            string hexa = Reutilizables.ByteArrayToHexString(resultado.ToArray());

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
                                var letra = Reutilizables.StringToByteArray(item);
                                bytes.Add(letra.ElementAt(0));
                            }

                            await btSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count, token);


                            CrossToastPopUp.Current.ShowToastSuccess($"trama: {Trama} enviada");
                            Trama = string.Empty;
                            await ReceivedData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                Debug.Write(ex.Message);
                btSocket.Close();
            }
            finally
            {
                tcs.Cancel();
            }
        }

        private async Task ReceivedData()
        {
            string tramaHexa = string.Empty;
            if (btSocket == null)
            {
                CrossToastPopUp.Current.ShowToastError($"Error: Conectese al bluetooth correspondiente.");
                return;
            }
            else
            {
                var inputstream = btSocket.InputStream;
                byte[] bufferRecibido = new byte[10];  // buffer store for the stream
                List<string> tramaDividida = new List<string>();

                int recibido = 0; // bytes returned from read()
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            recibido = await inputstream.ReadAsync(bufferRecibido, 0, bufferRecibido.Length);

                            if (recibido > 0)
                            {
                                byte[] rebuf2 = new byte[recibido];
                                Array.Copy(bufferRecibido, 0, rebuf2, 0, recibido);
                                tramaHexa += Reutilizables.ByteArrayToHexString(rebuf2);
                                if (tramaHexa.Length == 20)
                                {
                                    break;
                                }
                            }
                            Thread.Sleep(100);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("no se pudo recibir la data." + e.Message);
                            btSocket.Close();
                            break;
                        }
                    }
                    tramaDividida = Split(tramaHexa, 2).ToList();

                    List<byte> temperatura = new List<byte>();

                    for (int i = 0; i < tramaDividida.Count; i++)
                    {
                        if (i == 4 || i == 5 || i == 6 || i == 7)
                        {
                            byte[] byteTemperatura = new byte[1];
                            byteTemperatura = Reutilizables.StringToByteArray(tramaDividida[i]);
                            temperatura.Add(byteTemperatura.ElementAt(0));
                        }
                    }
                    TramaRecibida = Reutilizables.ByteToIEEEFloatingPoint(temperatura.ToArray()).ToString();
                });

            }

        }
        public static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}
