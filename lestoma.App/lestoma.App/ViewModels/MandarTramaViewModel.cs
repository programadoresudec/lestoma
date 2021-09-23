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
        private Java.Lang.String dataToSend;
        private BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothSocket btSocket = null;
        private Stream outStream = null;
        private Stream inStream = null;
        BluetoothServerSocket bthServerSocket = null;
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

        private async Task writeDataAsync(Java.Lang.String data)
        {
            //Extraemos el stream de salida
            try
            {
                outStream = btSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error al enviar", e.Message, "OK");

            }

            //creamos el string que enviaremos
            Java.Lang.String message = data;

            //lo convertimos en bytes
            byte[] msgBuffer = message.GetBytes();

            try
            {
                //Escribimos en el buffer el arreglo que acabamos de generar
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error al enviar", e.Message, "OK");
            }
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
                //Inicamos el socket de comunicacion con el arduino
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                //Conectamos el socket
                btSocket.Connect();
                if (btSocket.IsConnected)
                {
                    await Application.Current.MainPage.DisplayAlert("bien", "conexion establecida", "OK");
                }
            }
            catch (System.Exception e)
            {
                //en caso de generarnos error cerramos el socket
                System.Console.WriteLine(e.Message);
                try
                {
                    btSocket.Close();
                }
                catch (System.Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");

                }
                Debug.WriteLine("socket establecido");
            }
            //Una vez conectados al bluetooth mandamos llamar el metodo que generara el hilo
            //que recibira los datos del arduino
            //beginListenForData();
            //NOTA envio la letra e ya que el sketch esta configurado para funcionar cuando
            //recibe esta letra.
            //  dataToSend = new Java.Lang.String("e");
            //writeData(dataToSend);
        }
        //Evento para inicializar el hilo que escuchara las peticiones del bluetooth
        public void beginListenForData()
        {
            //Extraemos el stream de entrada
            try
            {
                inStream = btSocket.InputStream;
            }
            catch (System.IO.IOException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            //Creamos un hilo que estara corriendo en background el cual verificara si hay algun dato
            //por parte del arduino

            Task.Factory.StartNew(async () =>
            {
                //declaramos el buffer donde guardaremos la lectura
                byte[] buffer = new byte[1024];
                //declaramos el numero de bytes recibidos
                int bytes;
                while (true)
                {
                    try
                    {
                        //leemos el buffer de entrada y asignamos la cantidad de bytes entrantes
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        //Verificamos que los bytes contengan informacion
                        if (bytes > 0)
                        {
                            //Corremos en la interfaz principal
                            await Task.Run(() =>
                            {
                                //Convertimos el valor de la informacion llegada a string
                                string valor = System.Text.Encoding.ASCII.GetString(buffer);
                                //Agregamos a nuestro label la informacion llegada
                                Trama = Trama + "\n" + valor;
                            });
                        }
                    }
                    catch (Java.IO.IOException)
                    {
                        //En caso de error limpiamos nuestra label y cortamos el hilo de comunicacion
                        await Task.Run(() =>
                        {
                            Trama = string.Empty;
                        });
                        break;
                    }
                }
            });
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
        private void writeData(Java.Lang.String data)
        {
            //Extraemos el stream de salida
            try
            {
                outStream = btSocket.OutputStream;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }

            //creamos el string que enviaremos
            Java.Lang.String message = data;

            //lo convertimos en bytes
            byte[] msgBuffer = message.GetBytes();

            try
            {
                //Escribimos en el buffer el arreglo que acabamos de generar
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
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
