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
        private readonly ICRCHelper _crcHelper;
        private float _valorTemperatura;
        private LaboratorioRequest _laboratorioRequest;
        private TramaComponenteRequest _componenteRequest;
        public LecturaSensorViewModel(INavigationService navigationService, IApiService apiService, IUnitOfWork unitOfWork, ICRCHelper crcHelper) :
            base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            _componenteRequest = new TramaComponenteRequest();
            _laboratorioRequest = new LaboratorioRequest();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _crcHelper = crcHelper;
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
                Title = $"Lectura {TramaComponente.NombreComponente}";
                var tramaAEnviar = _crcHelper.TramaConCRC16Modbus(TramaComponente.TramaOchoBytes);
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
                if (!MBluetoothAdapter.IsEnabled)
                {
                    AlertError("Debe prender el bluetooth.");
                    return;
                }
                if (btSocket.IsConnected)
                {
                    Timer timer = new Timer(state => _cancellationTokenSource.Cancel(), null, 30000, Timeout.Infinite);
                    await btSocket.OutputStream.WriteAsync(tramaEnviada.ToArray(), 0, tramaEnviada.Count, _cancellationToken);
                    var tramaRecibida = await ReceivedData();
                    if (string.IsNullOrWhiteSpace(tramaRecibida))
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "No se pudo obtener la trama.", icon: Constants.ICON_WARNING));
                        return;
                    }
                    var response = _crcHelper.VerifyCRCOfReceivedTrama(tramaRecibida);
                    if (!response.IsExito)
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: response.MensajeHttp, icon: Constants.ICON_WARNING));
                        return;
                    }
                    Valor = Reutilizables.ConvertReceivedTramaToResult(tramaRecibida);
                    SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), tramaRecibida);
                }
            }
            catch (Exception ex)
            {
                btSocket?.Close();
                SeeError(ex);
            }
        }

        private async Task<string> ReceivedData()
        {
            string TramaHexadecimal = string.Empty;
            var inputstream = btSocket.InputStream;
            // buffer store for the stream
            byte[] bufferRecibido = new byte[Constants.BYTE_TRAMA_LENGTH];
            // bytes returned from read()
            int recibido = 0;
            return await Task.Run(async () =>
            {
                UserDialogs.Instance.ShowLoading("Cargando información...");
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
                            if (!string.IsNullOrWhiteSpace(TramaHexadecimal))
                            {
                                if (TramaHexadecimal.Length == Constants.HEXADECIMAL_TRAMA_LENGTH)
                                {
                                    _cancellationTokenSource.Cancel();
                                }
                            }
                        }
                        // Esta pausa es útil para evitar una lectura excesivamente rápida o continua del flujo de entrada.
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        SeeError(ex);
                        btSocket?.Close();
                    }
                    finally
                    {
                        UserDialogs.Instance.HideLoading();
                    }
                }
                return TramaHexadecimal;
            }, _cancellationToken);
        }
        private async void SaveData(string TramaEnviada, string tramaRecibida)
        {
            try
            {
               
                _laboratorioRequest.TramaEnviada = TramaEnviada;
                _laboratorioRequest.TramaRecibida = tramaRecibida;
                _laboratorioRequest.ComponenteId = _componenteRequest.ComponenteId;
                _laboratorioRequest.SetPointOut = Valor;
                _laboratorioRequest.Session = TokenUser.User.FullName;
                _laboratorioRequest.TipoDeAplicacion = EnumConfig.GetDescription(TipoAplicacion.AppMovil);
                if (_apiService.CheckConnection())
                {
                    LestomaLog.Normal("Enviando al servidor.");
                    UserDialogs.Instance.ShowLoading("Enviando al servidor...");
                    _laboratorioRequest.EstadoInternet = true;
                    _laboratorioRequest.Ip = await GetPublicIPAddressAsync();
                    ResponseDTO response = await _apiService.PostAsyncWithToken(URL_API, "laboratorio-lestoma/crear-detalle",
                        _laboratorioRequest, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertError(response.MensajeHttp);
                        return;
                    }
                    AlertSuccess("Lectura recibida satisfactoriamente.");
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
                    AlertSuccess("Lectura recibida satisfactoriamente.");
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
