using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace lestoma.App.ViewModels.Laboratorio
{
    public class LecturaSensorViewModel : BaseViewModel
    {
        CancellationTokenSource tcs = new CancellationTokenSource();
        CancellationToken token = new CancellationToken();
        private readonly IApiService _apiService;
        private float _valorTemperatura;
        private LaboratorioRequest _laboratorioRequest;
        private TramaComponenteRequest _componenteRequest;
        public LecturaSensorViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;
            _componenteRequest = new TramaComponenteRequest();
            _laboratorioRequest = new LaboratorioRequest();
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

                    var tramaRecibida = await ReceivedData();
                    if (string.IsNullOrWhiteSpace(tramaRecibida))
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "No se pudo obtener la trama.", icon: Constants.ICON_WARNING));
                        return;
                    }
                    Valor = Reutilizables.ConvertReceivedTramaToResult(tramaRecibida);
                    SaveData(Reutilizables.ByteArrayToHexString(tramaCompleta.ToArray()), tramaRecibida);
                }
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                btSocket.Close();
                SeeError(ex);
            }
            finally
            {
                tcs.Cancel();
            }
        }

        private async void SaveData(string TramaEnviada, string tramaRecibida)
        {
            try
            {
                _laboratorioRequest.Ip = GetLocalIPAddress();
                _laboratorioRequest.TramaEnviada = TramaEnviada;
                _laboratorioRequest.TramaRecibida = tramaRecibida;
                _laboratorioRequest.ComponenteId = _componenteRequest.ComponenteId;
                if (_apiService.CheckConnection())
                {
                    _laboratorioRequest.EstadoInternet = true;
                    _laboratorioRequest.SetPointOut = Valor;
                    UserDialogs.Instance.ShowLoading("Enviando al servidor...");
                    ResponseDTO response = await _apiService.PostAsyncWithToken(URL_API, "laboratorio-lestoma/crear-detalle",
                        _laboratorioRequest, TokenUser.Token);
                    if (response.IsExito)
                    {
                        AlertError(response.MensajeHttp);
                        return;
                    }
                    AlertSuccess(response.MensajeHttp);
                }
                else
                {
                    _laboratorioRequest.EstadoInternet = false;
                    _laboratorioRequest.FechaCreacionDispositivo = DateTime.Now;
                    UserDialogs.Instance.ShowLoading("Guardando localmente...");
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async Task<string> ReceivedData()
        {
            string TramaHexadecimal = string.Empty;
            var inputstream = btSocket.InputStream;
            byte[] bufferRecibido = new byte[10];  // buffer store for the stream
            int recibido = 0; // bytes returned from read()
            return await Task.Run(async () =>
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
                return TramaHexadecimal;
            });
        }
    }
}
