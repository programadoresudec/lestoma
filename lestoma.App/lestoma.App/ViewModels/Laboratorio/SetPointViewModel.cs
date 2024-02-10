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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class SetPointViewModel : BaseViewModel
    {
        CancellationTokenSource _cancellationTokenSource;
        CancellationToken _cancellationToken;
        private readonly IApiService _apiService;
        private LaboratorioRequest _laboratorioRequest;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICRCHelper _crcHelper;
        private TramaComponenteSetPointRequest _componenteRequest;
        public SetPointViewModel(INavigationService navigationService, IApiService apiService,
            IUnitOfWork unitOfWork, ICRCHelper crcHelper) :
            base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            _componenteRequest = new TramaComponenteSetPointRequest();
            _laboratorioRequest = new LaboratorioRequest();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _crcHelper = crcHelper;
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("tramaComponente"))
            {
                TramaComponente = parameters.GetValue<TramaComponenteSetPointRequest>("tramaComponente");
                Title = $"Ajuste {TramaComponente.NombreComponente}";
                SendTrama(TramaComponente.TramaWithCRC);
            }
        }

        public TramaComponenteSetPointRequest TramaComponente
        {
            get => _componenteRequest;
            set => SetProperty(ref _componenteRequest, value);
        }

        private void SendTrama(List<byte> tramaEnviada)
        {
            try
            {
                if (_apiService.CheckConnection() && btSocket == null)
                {
                    SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), Constants.TRAMA_SUCESS, (float)HttpStatusCode.OK);
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
            Debug.WriteLine("Conectado..");
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
                UserDialogs.Instance.ShowLoading("Enviando información...");
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
                        btSocket?.Close();
                        SeeError(ex);
                    }
                    finally
                    {
                        UserDialogs.Instance.HideLoading();
                    }
                }
                return TramaHexadecimal;
            }, _cancellationToken);
        }
        private async void SaveData(string TramaEnviada, string tramaRecibida, float SetPointOut)
        {
            try
            {
                _laboratorioRequest.TramaEnviada = TramaEnviada;
                _laboratorioRequest.TramaRecibida = tramaRecibida;
                _laboratorioRequest.ComponenteId = _componenteRequest.ComponenteId;
                _laboratorioRequest.SetPointIn = TramaComponente.ValorSetPoint;
                _laboratorioRequest.SetPointOut = SetPointOut;
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
                    AlertSuccess("Ajuste realizado.");
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
                    AlertSuccess("Ajuste realizado.");
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
