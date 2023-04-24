using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.App.Views.Laboratorio;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Requests.Filters;
using lestoma.DatabaseOffline.IConfiguration;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class ComponentesModuloViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<ComponentePorModuloDTO> _componentes;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private ObservableCollection<NameProtocoloDTO> _protocolos;
        private NameProtocoloDTO _protocolo;
        private bool _isSuperAdmin;
        private Guid _moduloId;
        private int? _esclavo;
        private bool _isEnabled = true;
        private bool _isNavigating = false;
        private readonly IUnitOfWork _unitOfWork;
        public ComponentesModuloViewModel(INavigationService navigationService, IApiService apiService, IUnitOfWork unitOfWork) :
             base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            Upas = new ObservableCollection<NameDTO>();
            _componentes = new ObservableCollection<ComponentePorModuloDTO>();
            _protocolos = new ObservableCollection<NameProtocoloDTO>();
            _protocolo = new NameProtocoloDTO();
            RedirectionTramaCommand = new Command<object>(ComponentSelected, CanNavigate);
            Title = "Componentes laboratorio";
            Bytes = LoadBytes();
            MessageHelp = _isSuperAdmin ? "Seleccione UPA, protocolo de comunicación y número de esclavo.\n\n Después de clic en alguno de los componentes para LECTURA, AJUSTE Y ON-OFF del laboratorio."
                                        : "Seleccione protocolo de comunicación y número de esclavo.\n\n Después de clic en alguno de los componentes para LECTURA, AJUSTE Y ON-OFF del laboratorio.";
        }
        #region Properties
        public Command RedirectionTramaCommand { get; set; }

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;

            set => SetProperty(ref _upas, value);
        }


        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                ListarProtocolos(_upa.Id);
                LoadComponents(_upa.Id);
            }
        }
        public ObservableCollection<NameProtocoloDTO> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }


        public NameProtocoloDTO Protocolo
        {
            get => _protocolo;
            set
            {
                SetProperty(ref _protocolo, value);
                if (_protocolo != null && !string.IsNullOrWhiteSpace(_protocolo.Nombre))
                {
                    if (_protocolo.Nombre.ToUpper().Contains("BROAD") || _protocolo.Nombre.ToUpper().Equals("BROADCAST"))
                    {
                        Esclavo = 0;
                        IsEnabled = false;
                    }
                    else
                    {
                        IsEnabled = true;
                        Esclavo = null;
                    }
                }
            }
        }

        public ObservableCollection<ComponentePorModuloDTO> Componentes
        {
            get => _componentes;
            set => SetProperty(ref _componentes, value);
        }
        public List<int> Bytes { get; set; }

        public int? Esclavo
        {
            get => _esclavo;
            set => SetProperty(ref _esclavo, value);
        }

        private List<int> LoadBytes()
        {
            return Enumerable.Range(0, 10).ToList();
        }

        #endregion

        #region Methods
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("ModuloId"))
            {
                _moduloId = parameters.GetValue<Guid>("ModuloId");
                LoadUpas();
            }
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }
        private async void LoadUpas()
        {
            try
            {
                Upas.Clear();
                UserDialogs.Instance.ShowLoading("Cargando...");
                // consume service en la nube
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                    if (response.IsExito)
                    {
                        Upas = new ObservableCollection<NameDTO>((List<NameDTO>)response.Data);
                    }
                    if (!IsSuperAdmin)
                    {
                        ResponseDTO upaAsignada = await _apiService.GetAsyncWithToken(URL_API, "usuarios/upa-asignada", TokenUser.Token);
                        if (response.IsExito)
                        {
                            var upa = ParsearData<NameDTO>(upaAsignada);
                            var selected = Upas.Where(x => x.Id == upa.Id).FirstOrDefault();
                            Upa = selected;
                            ListarComponentesOnline(upa.Id);
                            ListarProtocolos(upa.Id);
                        }
                    }
                }
                // consume service en la bd del dispositivo movil
                else
                {
                    var data = await _unitOfWork.Componentes.GetUpas();
                    Upas = new ObservableCollection<NameDTO>(data);

                    if (!IsSuperAdmin)
                    {
                        var selected = Upas.Where(x => x.Id == Upas[0].Id).FirstOrDefault();
                        Upa = selected;
                        ListarProtocolos(Upa.Id);
                        ListarComponentesOffline(Upa.Id);

                    }
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
        private void LoadComponents(Guid upaId)
        {
            if (_apiService.CheckConnection())
            {
                ListarComponentesOnline(upaId);
            }
            else
            {
                ListarComponentesOffline(upaId);
            }
        }


        private async void ListarComponentesOffline(Guid upaId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                Componentes = new ObservableCollection<ComponentePorModuloDTO>();
                var listado = await _unitOfWork.Componentes.GetComponentesPorModuloUpa(upaId, _moduloId);
                if (!listado.Any())
                {
                    AlertWarning("No hay componentes con la upa seleccionada.");
                    return;
                }
                Componentes = new ObservableCollection<ComponentePorModuloDTO>(listado);
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


        private async void ListarComponentesOnline(Guid upaId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                Componentes = new ObservableCollection<ComponentePorModuloDTO>();

                var upaModuleFilterRequest = new UpaModuleFilterRequest()
                {
                    ModuloId = _moduloId,
                    UpaId = upaId
                };
                string querystring = Reutilizables.GenerateQueryString(upaModuleFilterRequest);
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ComponentePorModuloDTO>>(URL_API,
                    $"laboratorio-lestoma/listar-componentes-upa-modulo{querystring}", TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertWarning(response.MensajeHttp);
                    return;
                }
                var listado = (List<ComponentePorModuloDTO>)response.Data;
                if (listado.Count == 0)
                {
                    AlertWarning("No hay componentes con la upa seleccionada.");
                    return;
                }
                Componentes = new ObservableCollection<ComponentePorModuloDTO>(listado);
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
        private async void ListarProtocolos(Guid UpaId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameProtocoloDTO>>(URL_API,
                        $"upas/listar-nombres-protocolo/{UpaId}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        Protocolos = new ObservableCollection<NameProtocoloDTO>((List<NameProtocoloDTO>)response.Data);
                    }
                }
                else
                {
                    var protocolos = await _unitOfWork.Componentes.GetProtocolos(UpaId);
                    Protocolos = new ObservableCollection<NameProtocoloDTO>(protocolos);
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

        private async void ComponentSelected(object objeto)
        {
            try
            {
                if (!_isNavigating)
                {
                    _isNavigating = true;
                    if (!AreFieldsValid())
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage(@$"Error: Todos los campos son obligatorios.", Constants.ICON_WARNING));
                        return;
                    }
                    var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
                    if (!(lista.ItemData is ComponentePorModuloDTO componente))
                        return;
                    var request = new TramaComponenteRequest
                    {
                        ComponenteId = componente.Id,
                        NombreComponente = componente.Nombre,
                        TramaOchoBytes = new List<byte>()
                                     {
                                         Protocolo.PrimerByteTrama,
                                         (byte)Esclavo.Value,
                                         componente.EstadoComponente.ByteDecimalFuncion,
                                         componente.DireccionRegistro,
                                         0,
                                         0,
                                         0,
                                         0
                                     }
                    };

                    var parameters = new NavigationParameters
                       {
                           { "tramaComponente", request }
                       };

                    if (EnumConfig.GetDescription(TipoEstadoComponente.Lectura).Equals(componente.EstadoComponente.TipoEstado))
                    {
                        await NavigationService.NavigateAsync(nameof(LecturaSensorPage), parameters);
                    }
                    else if (EnumConfig.GetDescription(TipoEstadoComponente.OnOff).Equals(componente.EstadoComponente.TipoEstado))
                    {
                        await NavigationService.NavigateAsync(nameof(EstadoActuadorPage), parameters);
                    }
                    else if (EnumConfig.GetDescription(TipoEstadoComponente.Ajuste).Equals(componente.EstadoComponente.TipoEstado))
                    {
                        await NavigationService.NavigateAsync(nameof(InputSetPointPopupPage), parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private bool AreFieldsValid()
        {
            bool isUpaValid = Upa != null;
            bool isProtocoloValid = Protocolo != null;
            bool isEsclavoValid = Esclavo != null;
            if (TokenUser.User.RolId == (int)TipoRol.Administrador)
            {
                isUpaValid = true;
            }
            return isUpaValid && isProtocoloValid && isEsclavoValid;
        }
        #endregion
    }
}