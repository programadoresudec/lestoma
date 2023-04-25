using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.IConfiguration;
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
        CancellationTokenSource _cancellationTokenSource;
        CancellationToken _cancellationToken;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApiService _apiService;
        private float _valorTemperatura;
        private LaboratorioRequest _laboratorioRequest;
        private TramaComponenteRequest _componenteRequest;
        public LecturaSensorViewModel(INavigationService navigationService, IApiService apiService, IUnitOfWork unitOfWork) :
            base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            _componenteRequest = new TramaComponenteRequest();
            _laboratorioRequest = new LaboratorioRequest();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
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
                Title = $"Estado {TramaComponente.NombreComponente}";
                var tramaAEnviar = Reutilizables.TramaConCRC16Modbus(TramaComponente.TramaOchoBytes);
                SendTrama(tramaAEnviar);
            }
        }

        private async void SendTrama(List<byte> tramaEnviada)
        {
            try
            {
                if (_apiService.CheckConnection() && btSocket == null)
                {
                    ResponseDTO response = await _apiService.GetAsyncWithToken(URL_API,
                        $"laboratorio-lestoma/ultimo-registro-componente/{TramaComponente.ComponenteId}", TokenUser.Token);
                    if (response.Data == null)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "No hay datos en el servidor de este componente por el momento.",
                                                       icon: Constants.ICON_WARNING));
                        return;
                    }
                    if (response.IsExito)
                    {
                        var data = ParsearData<TramaComponenteDTO>(response);
                        Valor = (float)data.SetPointOut.Value;
                        SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), data.TramaOutPut);
                    }
                }
                else if (_apiService.CheckConnection() && btSocket != null)
                {
                    LestomaLog.Normal("Conectado al Bluetooth.");
                    TransmissionBluetooth(tramaEnviada);
                }
                else if (!_apiService.CheckConnection())
                {
                    if (btSocket == null)
                    {
                        AlertError("No hay conexión a ningún Bluetooth.");
                        return;
                    }
                    LestomaLog.Normal("Conectado al Bluetooth.");
                    TransmissionBluetooth(tramaEnviada);
                }
            }
            catch (Exception ex)
            {
                btSocket?.Close();
                SeeError(ex);
            }
        }

        private async void TransmissionBluetooth(List<byte> tramaEnviada)
        {
            try
            {
                if (btSocket.IsConnected)
                {
                    await btSocket.OutputStream.WriteAsync(tramaEnviada.ToArray(), 0, tramaEnviada.Count, _cancellationToken);
                    var tramaRecibida = await ReceivedData();
                    if (string.IsNullOrWhiteSpace(tramaRecibida))
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "No se pudo obtener la trama.", icon: Constants.ICON_WARNING));
                        return;
                    }
                    Valor = Reutilizables.ConvertReceivedTramaToResult(tramaRecibida);
                    SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), tramaRecibida);
                }
            }
            catch (Exception ex)
            {
                btSocket.Close();
                SeeError(ex);
                AlertWarning("Se ha desconectado el bluetooth.");
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
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        recibido = await inputstream.ReadAsync(bufferRecibido, 0, bufferRecibido.Length, _cancellationToken);

                        if (recibido > 0)
                        {
                            byte[] rebuf2 = new byte[recibido];
                            Array.Copy(bufferRecibido, 0, rebuf2, 0, recibido);
                            TramaHexadecimal += Reutilizables.ByteArrayToHexString(rebuf2);
                            if (TramaHexadecimal.Length == 20)
                            {
                                _cancellationTokenSource.Cancel();
                            }
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        SeeError(ex, "No se pudo recibir la data de la trama por bluetooth.");
                        btSocket.Close();
                        throw;
                    }
                }
                return TramaHexadecimal;
            }, _cancellationToken);
        }
        private async void SaveData(string TramaEnviada, string tramaRecibida)
        {
            try
            {
                _laboratorioRequest.Ip = GetLocalIPAddress();
                _laboratorioRequest.TramaEnviada = TramaEnviada;
                _laboratorioRequest.TramaRecibida = tramaRecibida;
                _laboratorioRequest.ComponenteId = _componenteRequest.ComponenteId;
                _laboratorioRequest.SetPointOut = Valor;
                _laboratorioRequest.Session = TokenUser.User.FullName;
                _laboratorioRequest.TipoDeAplicacion = EnumConfig.GetDescription(TipoAplicacion.AppMovil);
                if (_apiService.CheckConnection())
                {
                    Debug.WriteLine("Enviando al servidor.");
                    LestomaLog.Normal("Enviando al servidor.");
                    _laboratorioRequest.EstadoInternet = true;
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
                    UserDialogs.Instance.ShowLoading("Guardando en el dispositivo...");
                    LestomaLog.Normal("Guardando en bd del dispositivo.");
                    ResponseDTO response = await _unitOfWork.Laboratorio.SaveDataOffline(_laboratorioRequest);
                    if (!response.IsExito)
                    {
                        AlertError(response.MensajeHttp);
                        return;
                    }
                    AlertSuccess(response.MensajeHttp);
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                _cancellationTokenSource.Cancel();
                UserDialogs.Instance.HideLoading();
            }
        }


    }
}
