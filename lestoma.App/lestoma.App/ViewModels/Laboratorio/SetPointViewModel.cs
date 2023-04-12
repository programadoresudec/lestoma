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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class SetPointViewModel : BaseViewModel
    {
        CancellationTokenSource tcs = new CancellationTokenSource();
        CancellationToken token = new CancellationToken();
        private readonly IApiService _apiService;
        private LaboratorioRequest _laboratorioRequest;
        private TramaComponenteSetPointRequest _componenteRequest;
        public SetPointViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;
            _componenteRequest = new TramaComponenteSetPointRequest();
            _laboratorioRequest = new LaboratorioRequest();
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("tramaComponente"))
            {
                TramaComponente = parameters.GetValue<TramaComponenteSetPointRequest>("tramaComponente");
                SendTrama(TramaComponente.TramaWithCRC);
            }
        }

        public TramaComponenteSetPointRequest TramaComponente
        {
            get => _componenteRequest;
            set => SetProperty(ref _componenteRequest, value);
        }

        private async void SendTrama(List<byte> tramaEnviada)
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), Constants.TRAMA_SUCESS, (float)HttpStatusCode.OK);
                }
                else
                {
                    if (btSocket == null)
                    {
                        AlertError("No hay conexión a ningún Bluetooth.");
                        return;
                    }
                    Debug.WriteLine("Conectado..");
                    if (btSocket.IsConnected)
                    {
                        await btSocket.OutputStream.WriteAsync(tramaEnviada.ToArray(), 0, tramaEnviada.Count, token);

                        var tramaRecibida = await ReceivedData();

                        if (string.IsNullOrWhiteSpace(tramaRecibida))
                        {
                            await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "No se pudo obtener la trama.", icon: Constants.ICON_WARNING));
                            return;
                        }
                        var response = Reutilizables.VerifyCRCOfReceivedTrama(tramaRecibida);
                        if (!response.IsExito)
                        {
                            await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: response.MensajeHttp, icon: Constants.ICON_WARNING));
                            return;
                        }

                        var dato = Reutilizables.ConvertReceivedTramaToResult(tramaRecibida);
                        if (dato == (int)HttpStatusCode.Conflict)
                        {
                            await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "Ha ocurrido un error al recibir los datos."
                                                            , icon: Constants.ICON_WARNING));
                            return;
                        }
                        SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), tramaRecibida, dato);
                    }
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
        private async void SaveData(string TramaEnviada, string tramaRecibida, float SetPointOut)
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
                    _laboratorioRequest.SetPointIn = TramaComponente.ValorSetPoint;
                    _laboratorioRequest.SetPointOut = SetPointOut;
                    UserDialogs.Instance.ShowLoading("Enviando al servidor...");
                    ResponseDTO response = await _apiService.PostAsyncWithToken(URL_API, "laboratorio-lestoma/crear-detalle",
                        _laboratorioRequest, TokenUser.Token);
                    if (!response.IsExito)
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
    }
}
