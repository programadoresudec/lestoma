using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.App.Views.Componentes;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Requests.Filters;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class CreateOrEditComponentViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<NameDTO> _upas;
        private ObservableCollection<NameDTO> _modulos;
        private ObservableCollection<NameDTO> _actividades;
        private NameDTO _upa;
        private int? _direccionregistro;
        private NameDTO _modulo;
        private NameDTO _actividad;
        private InfoComponenteDTO _infoComponente;
        private EstadoComponenteDTO _estadoComponente;
        private bool _isCreated;
        private bool _isVisible;
        private string _iconStatusComponent;
        private bool _isSuperAdmin;
        private bool _isVisibleDireccionRegistro = true;
        private ObservableCollection<int> _direccionesNoUtilizadas;

        public CreateOrEditComponentViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            CreateOrEditCommand = new Command(CreateOrEditClicked);
            AddStatusComponentCommand = new Command(AddStatusComponentClicked);
            _infoComponente = new InfoComponenteDTO();
            _iconStatusComponent = "icon_create.png";
        }



        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("idComponent"))
            {
                var Id = parameters.GetValue<Guid>("idComponent");
                Title = "Editar Componente";
                _iconStatusComponent = "icon_edit.png";
                IsCreated = false;
                _isVisibleDireccionRegistro = false;
                LoadLists(Id);
            }
            else if (parameters.ContainsKey(Constants.REFRESH))
            {
                EstadoComponente = JsonConvert.DeserializeObject<EstadoComponenteDTO>(MovilSettings.EstadoComponente);
            }
            else
            {
                IsSuperAdmin = true;
                _iconStatusComponent = "icon_create.png";
                Title = "Agregar Componente";
                LoadLists(Guid.Empty);
            }
        }

        public Command AddStatusComponentCommand { get; }
        public Command CreateOrEditCommand { get; }

        public ObservableCollection<NameDTO> Modulos
        {
            get => _modulos;
            set => SetProperty(ref _modulos, value);
        }
        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }
        public EstadoComponenteDTO EstadoComponente
        {
            get => _estadoComponente;
            set => SetProperty(ref _estadoComponente, value);
        }

        public ObservableCollection<int> DireccionesNoUtilizadas
        {
            get => _direccionesNoUtilizadas;
            set => SetProperty(ref _direccionesNoUtilizadas, value);
        }

        public int? DireccionRegistro
        {
            get => _direccionregistro;
            set => SetProperty(ref _direccionregistro, value);
        }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }

        public ObservableCollection<NameDTO> Actividades
        {
            get => _actividades;
            set => SetProperty(ref _actividades, value);
        }

        public NameDTO Actividad
        {
            get => _actividad;
            set
            {
                SetProperty(ref _actividad, value);
                if (_isVisibleDireccionRegistro)
                {
                    LoadDireccionesDeRegistro();
                }

            }
        }
        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                if (_isVisibleDireccionRegistro)
                {
                    LoadDireccionesDeRegistro();
                }
            }
        }

        public NameDTO Modulo
        {
            get => _modulo;
            set
            {
                SetProperty(ref _modulo, value);
                if (_isVisibleDireccionRegistro)
                {
                    LoadDireccionesDeRegistro();
                }
            }
        }
        public InfoComponenteDTO InfoComponente
        {
            get => _infoComponente;
            set => SetProperty(ref _infoComponente, value);
        }
        public bool IsCreated
        {
            get => _isCreated;
            set => SetProperty(ref _isCreated, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        public string IconStatusComponent
        {
            get => _iconStatusComponent;
            set => SetProperty(ref _iconStatusComponent, value);
        }
        private async void LoadDireccionesDeRegistro()
        {
            try
            {
                if (Title.Contains("Crear") && TokenUser.User.RolId == (int)TipoRol.Administrador)
                {
                    _upa = new NameDTO();
                }
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (_apiService.CheckConnection() && _upa != null && _modulo != null && _actividad != null)
                {
                    IsCreated = true;
                    var upaModuleActivityFilterRequest = new UpaModuleActivityFilterRequest
                    {
                        UpaId = _upa.Id,
                        ModuloId = _modulo.Id,
                        ActividadId = _actividad.Id
                    };
                    var queryString = Reutilizables.GenerateQueryString(upaModuleActivityFilterRequest);

                    ResponseDTO direcciones = await _apiService.GetListAsyncWithToken<List<int>>(URL_API,
                        $"componentes/direcciones-de-registro-upa-modulo{queryString}", TokenUser.Token);

                    DireccionesNoUtilizadas = new ObservableCollection<int>((List<int>)direcciones.Data);
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

        private async void LoadLists(Guid id)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (!IsCreated)
                {
                    DireccionesNoUtilizadas = new ObservableCollection<int>(Enumerable.Range(0, 256).ToList());
                }

                if (_apiService.CheckConnection())
                {
                    ResponseDTO modulos = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                        "modulos/listar-nombres", TokenUser.Token);
                    Modulos = new ObservableCollection<NameDTO>((List<NameDTO>)modulos.Data);

                    if (TokenUser.User.RolId == (int)TipoRol.SuperAdministrador)
                    {
                        IsVisible = true;
                        ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                       "upas/listar-nombres", TokenUser.Token);
                        Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                        ResponseDTO actividades = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                        "actividades/listar-nombres", TokenUser.Token);
                        Actividades = new ObservableCollection<NameDTO>((List<NameDTO>)actividades.Data);
                    }
                    else
                    {
                        IsVisible = false;
                        ResponseDTO actividades = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                        $"detalle-upas-actividades/listar-por-usuario", TokenUser.Token);
                        Actividades = new ObservableCollection<NameDTO>((List<NameDTO>)actividades.Data);
                    }
                    if (id != Guid.Empty)
                    {
                        var infoComponente = await _apiService.GetAsyncWithToken(URL_API, $"componentes/{id}", TokenUser.Token);
                        if (infoComponente.IsExito)
                        {
                            InfoComponente = ParsearData<InfoComponenteDTO>(infoComponente);
                            DireccionRegistro = InfoComponente.DireccionDeRegistro;
                            var modulo = Modulos.Where(x => x.Id == InfoComponente.Modulo.Id).FirstOrDefault();
                            var upa = Upas?.Where(x => x.Id == InfoComponente.Upa.Id).FirstOrDefault();
                            var actividad = Actividades.Where(x => x.Id == InfoComponente.Actividad.Id).FirstOrDefault();

                            if (modulo != null && actividad != null)
                            {
                                Upa = upa;
                                Modulo = modulo;
                                Actividad = actividad;
                            }
                            EstadoComponente = InfoComponente.EstadoComponente;
                        }
                    }
                }
                else
                {
                    AlertNoInternetConnection();
                    await NavigationService.GoBackAsync();
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

        private async void AddStatusComponentClicked()
        {
            var data = new InfoEstadoComponenteModel
            {
                Estado = InfoComponente.Id != Guid.Empty ? InfoComponente.EstadoComponente : null,
                IsCreated = Title.ToUpper().Contains("AGREGAR")
            };
            if (!string.IsNullOrEmpty(MovilSettings.EstadoComponente))
            {
                data.Estado = JsonConvert.DeserializeObject<EstadoComponenteDTO>(MovilSettings.EstadoComponente);
            }
            var parameters = new NavigationParameters
            {
                {
                    "estadoComponente",
                    data
                }
            };
            await NavigationService.NavigateAsync($"{nameof(InfoEstadoPopupPage)}", parameters);
        }

        private async void CreateOrEditClicked(object obj)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Guardando...");
                if (!AreFieldsValid())
                {
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage(@$"Error: Todos los campos son obligatorios.", Constants.ICON_WARNING));
                    return;

                }
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                if (InfoComponente.Id == Guid.Empty)
                {
                    var createRequest = new CreateComponenteRequest
                    {
                        Nombre = InfoComponente.Nombre,
                        ActividadId = Actividad.Id,
                        ModuloComponenteId = Modulo.Id,
                        TipoEstadoComponente = InfoComponente.EstadoComponente,
                        DireccionRegistro = (byte)DireccionRegistro
                    };
                    if (Upa != null)
                    {
                        createRequest.UpaId = Upa.Id;
                    }
                    else
                    {
                        createRequest.UpaId = Guid.Empty;
                    }
                    ResponseDTO respuesta = await _apiService.PostAsyncWithToken(URL_API, "componentes/crear", createRequest, TokenUser.Token);
                    if (respuesta.IsExito)
                    {
                        AlertSuccess(respuesta.MensajeHttp);
                        var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                        await NavigationService.GoBackAsync(parameters);
                    }
                    else
                    {
                        AlertWarning(respuesta.MensajeHttp);
                    }
                }
                else
                {
                    var EditRequest = new EditComponenteRequest
                    {
                        Id = InfoComponente.Id,
                        TipoEstadoComponente = InfoComponente.EstadoComponente,
                        Nombre = InfoComponente.Nombre,
                        ActividadId = Actividad.Id,
                        UpaId = Upa != null ? Upa.Id : Guid.Empty,
                        ModuloComponenteId = Modulo.Id
                    };

                    ResponseDTO respuesta;
                    if (_isSuperAdmin)
                    {
                        respuesta = await _apiService.PutAsyncWithToken(URL_API, "componentes/editar-super-admin", EditRequest, TokenUser.Token);
                    }
                    else
                    {
                        respuesta = await _apiService.PutAsyncWithToken(URL_API, "componentes/editar-admin", EditRequest, TokenUser.Token);
                    }
                    if (respuesta.IsExito)
                    {
                        AlertSuccess(respuesta.MensajeHttp);
                        var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                        await NavigationService.GoBackAsync(parameters);
                    }
                    else
                    {
                        AlertWarning(respuesta.MensajeHttp);
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

        private bool AreFieldsValid()
        {
            InfoComponente.EstadoComponente = !string.IsNullOrWhiteSpace(MovilSettings.EstadoComponente)
           ? JsonConvert.DeserializeObject<EstadoComponenteDTO>(MovilSettings.EstadoComponente)
           : null;
            bool isNameValid = !string.IsNullOrWhiteSpace(InfoComponente.Nombre);
            bool isUpaValid = Upa != null;
            bool isActividadValid = Actividad != null;
            bool isModuloValid = Modulo != null;
            bool isDireccionValid = DireccionRegistro != null;
            bool isEstadoValid = InfoComponente.EstadoComponente != null;
            if (TokenUser.User.RolId == (int)TipoRol.Administrador)
            {
                isUpaValid = true;
            }
            return isNameValid && isDireccionValid && isEstadoValid && isUpaValid && isModuloValid && isActividadValid;
        }
    }
}