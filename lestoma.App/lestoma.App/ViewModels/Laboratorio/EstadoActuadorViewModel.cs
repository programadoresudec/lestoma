using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.ListadosJson;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.IConfiguration;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class EstadoActuadorViewModel : BaseViewModel
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApiService _apiService;
        private readonly ICRCHelper _crcHelper;
        private float? _valorOnOff;
        private LaboratorioRequest _laboratorioRequest;
        private TramaComponenteRequest _componenteRequest;
        private bool _isEnabled;
        private Boolean? _IsOn;
        public EstadoActuadorViewModel(INavigationService navigationService, IApiService apiService,
            IUnitOfWork unitOfWork, ICRCHelper crcHelper) :
            base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            _componenteRequest = new TramaComponenteRequest();
            _laboratorioRequest = new LaboratorioRequest();
            _isEnabled = TokenUser.User.RolId != (int)TipoRol.Auxiliar;
            ChangeStatedCommand = new Command<object>(StatedSelected, CanNavigate);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _crcHelper = crcHelper;
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }
        public Command ChangeStatedCommand { get; set; }

        public TramaComponenteRequest TramaComponente
        {
            get => _componenteRequest;
            set => SetProperty(ref _componenteRequest, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public float? Valor
        {
            get => _valorOnOff;
            set => SetProperty(ref _valorOnOff, value);
        }

        public Boolean? IsOn
        {
            get => _IsOn;
            set => SetProperty(ref _IsOn, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("tramaComponente"))
            {
                TramaComponente = parameters.GetValue<TramaComponenteRequest>("tramaComponente");
                Title = $"ON/OFF {TramaComponente.NombreComponente}";
                var tramaAEnviar = _crcHelper.TramaConCRC16Modbus(new List<byte>(TramaComponente.TramaOchoBytes));
                SendTrama(tramaAEnviar);
            }
        }
        private void StatedSelected(object obj)
        {
            try
            {
                if (!IsEnabled)
                {
                    AlertWarning("No tiene permisos para realizar esta acción.");
                    IsOn = !IsOn;
                    return;
                }
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
                byte[] bytesFlotante = new byte[4];
                TramaComponente.TramaOchoBytes[2] = new ListadoEstadoComponente().GetEstadoAjuste().ByteDecimalFuncion;
                TramaComponente.TramaOchoBytes[4] = (byte)(IsOn.Value ? 1 : 0);

                var tramaAEnviar = _crcHelper.TramaConCRC16Modbus(new List<byte>(TramaComponente.TramaOchoBytes));
                SendTrama(tramaAEnviar, true);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }
        private async void SendTrama(List<byte> tramaEnviada, bool EditState = false)
        {
            try
            {
                if (_apiService.CheckConnection() && btSocket == null)
                {
                    if (!EditState)
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
                            if (data.SetPointOut != null && data.SetPointOut != (int)HttpStatusCode.OK)
                            {
                                Valor = (float)data.SetPointOut.Value;
                            }
                            if (data.SetPointIn != null)
                            {
                                Valor = (float)data.SetPointIn.Value;
                            }
                            if (EditState && Valor == (int)HttpStatusCode.Conflict)
                            {
                                await PopupNavigation.Instance.PushAsync(
                                    new MessagePopupPage(message: "No se pudo obtener el estado, ha ocurrido un error al recibir los datos.",
                                    icon: Constants.ICON_WARNING));
                                return;
                            }
                            IsOn = Valor == 1;
                            SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), data.TramaOutPut, EditState);
                        }
                    }
                    else
                    {
                        SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), Constants.TRAMA_SUCESS, EditState);
                    }
                }
                else if (_apiService.CheckConnection() && btSocket != null)
                {
                    LestomaLog.Normal("Conectado al Bluetooth.");
                    TransmissionBluetooth(tramaEnviada, EditState);
                }
                else if (!_apiService.CheckConnection())
                {
                    if (btSocket == null)
                    {
                        AlertError("No hay conexión a ningún Bluetooth.");
                        return;
                    }
                    LestomaLog.Normal("Conectado al Bluetooth.");
                    TransmissionBluetooth(tramaEnviada, EditState);
                }
            }
            catch (Exception ex)
            {
                btSocket?.Close();
                SeeError(ex);
            }
        }

        private async void TransmissionBluetooth(List<byte> tramaEnviada, bool editState)
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

                    var tramaRecibida = await ReceivedData(editState);
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
                    if (editState)
                    {
                        if (!tramaRecibida.Equals(Constants.TRAMA_SUCESS))
                        {
                            string message = IsOn == true ? "encender" : "apagar";
                            await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: $"No se pudo {message}, ha ocurrido un error al recibir los datos."
                                                            , icon: Constants.ICON_WARNING));
                            IsOn = !IsOn.Value;
                            return;
                        }
                    }
                    else
                    {
                        if (Valor.HasValue)
                        {
                            int OnOff = int.Parse(Valor.Value.ToString());
                            if (OnOff > 1 && OnOff < 0)
                            {
                                await PopupNavigation.Instance.PushAsync(new MessagePopupPage(message: "No se pudo obtener el estado, ha ocurrido un error al recibir los datos."
                                                           , icon: Constants.ICON_WARNING));
                                return;
                            }
                            IsOn = OnOff == 1;
                        }
                    }
                    SaveData(Reutilizables.ByteArrayToHexString(tramaEnviada.ToArray()), tramaRecibida, editState);
                }
            }
            catch (Exception ex)
            {
                btSocket?.Close();
                SeeError(ex);
            }
        }



        private async Task<string> ReceivedData(bool editState)
        {
            string TramaHexadecimal = string.Empty;
            var inputstream = btSocket.InputStream;
            // buffer store for the stream
            byte[] bufferRecibido = new byte[Constants.BYTE_TRAMA_LENGTH];
            // bytes returned from read()
            int recibido = 0;
            return await Task.Run(async () =>
            {
                if (editState)
                {
                    UserDialogs.Instance.ShowLoading("Enviando información...");
                }
                else
                {
                    UserDialogs.Instance.ShowLoading("Cargando información...");
                }

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
        private async void SaveData(string TramaEnviada, string tramaRecibida, bool EditState)
        {
            try
            {
                _laboratorioRequest.TramaEnviada = TramaEnviada;
                _laboratorioRequest.TramaRecibida = tramaRecibida;
                _laboratorioRequest.ComponenteId = _componenteRequest.ComponenteId;
                _laboratorioRequest.SetPointOut = Valor;
                _laboratorioRequest.Session = TokenUser.User.FullName;
                _laboratorioRequest.TipoDeAplicacion = EnumConfig.GetDescription(TipoAplicacion.AppMovil);
                if (EditState)
                {
                    _laboratorioRequest.SetPointIn = IsOn.Value ? 1 : 0;
                }
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
                    if (EditState)
                    {
                        string onOff = IsOn.Value ? "encendido" : "apagado";
                        AlertSuccess($"Acción realizada, {onOff}");
                    }
                    else
                    {
                        AlertSuccess("Lectura recibida satisfactoriamente.");
                    }
                }
                else
                {
                    UserDialogs.Instance.ShowLoading("Guardando en el dispositivo...");
                    LestomaLog.Normal("Guardando en bd del dispositivo.");
                    var response = await _unitOfWork.Laboratorio.SaveDataOffline(_laboratorioRequest);
                    if (!response.IsExito)
                    {
                        AlertError(response.MensajeHttp);
                        return;
                    }
                    if (EditState)
                    {
                        string onOff = IsOn.Value ? "encendido" : "apagado";
                        AlertSuccess($"Acción realizada, {onOff}");
                    }
                    else
                    {
                        AlertSuccess("Lectura recibida satisfactoriamente.");
                    }
                }
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
    }
}
