using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Requests;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace lestoma.App.ViewModels.Laboratorio
{
    public class LecturaSensorViewModel : BaseViewModel
    {
        CancellationTokenSource tcs = new CancellationTokenSource();
        CancellationToken token = new CancellationToken();

        private float _valorTemperatura;
        private TramaComponenteRequest _componenteRequest;
        public LecturaSensorViewModel(INavigationService navigationService) : 
            base(navigationService)
        {
            _componenteRequest  = new TramaComponenteRequest();
        }

        public TramaComponenteRequest TramaComponente
        {
            get => _componenteRequest;
            set => SetProperty(ref _componenteRequest, value);
        }

        public float Valor
        {
            get => _valorTemperatura;
            set => SetProperty(ref _valorTemperatura, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("tramaComponente"))
            {
                TramaComponente = parameters.GetValue<TramaComponenteRequest>("tramaComponente");
                var tramaCompleta = Reutilizables.TramaConCRC(TramaComponente.TramaOchoBytes);
                SendTrama(tramaCompleta);
            }
        }
        private async void SendTrama(List<byte> tramaCompleta)
        {
            try
            {
                if (btSocket == null)
                {
                    AlertError("No hay conexión a ningún Bluetooth.");
                    return;
                }
                Debug.WriteLine("Conectado");
                if (btSocket.IsConnected)
                {
                    await btSocket.OutputStream.WriteAsync(tramaCompleta.ToArray(), 0, tramaCompleta.Count, token);
                    ReceivedData();
                }
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                btSocket.Close();
            }
            finally
            {
                tcs.Cancel();
            }
        }

        private async void ReceivedData()
        {
            if (btSocket == null)
            {
                CrossToastPopUp.Current.ShowToastError($"Error: Conectese al bluetooth correspondiente.");
                return;
            }
            string TramaHexadecimal = string.Empty;
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
                            TramaHexadecimal += Reutilizables.ByteArrayToHexString(rebuf2);
                            if (TramaHexadecimal.Length == 20)
                            {
                                break;
                            }
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        SeeError(ex);
                        LestomaLog.Error(ex.Message);
                        Debug.WriteLine("no se pudo recibir la data." + ex.Message);
                        btSocket.Close();
                        break;
                    }
                }
                tramaDividida = Reutilizables.Split(TramaHexadecimal, 2).ToList();

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
                var valor = Reutilizables.ByteToIEEEFloatingPoint(temperatura.ToArray());
                Valor = valor;
            });
        }
    }
}
