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
        private NameDTO _modulo;
        private NameDTO _actividad;
        private InfoComponenteDTO _infoComponente;
        private bool _isEdit;
        private bool _isVisible;
        private string _iconStatusComponent;
        private string _jsonEstadoComponente;

        public CreateOrEditComponentViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;
            CreateOrEditCommand = new Command(CreateOrEditClicked);
            AddStatusComponentCommand = new Command(AddStatusComponentClicked);
            _infoComponente = new InfoComponenteDTO();
            _isEdit = true;
            _iconStatusComponent = "icon_create.png";
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("idComponent"))
            {
                var Id = parameters.GetValue<Guid>("idComponent");
                Title = "Editar";
                _iconStatusComponent = "icon_edit.png";
                IsEdit = false;
                LoadLists(Id);
            }
            else if (parameters.ContainsKey(Constants.REFRESH))
            {
                var json = JsonConvert.DeserializeObject<EstadoComponenteDTO>(MovilSettings.EstadoComponente);
                JsonEstadoComponente = JsonConvert.SerializeObject(json, Formatting.Indented);
            }
            else
            {
                _iconStatusComponent = "icon_create.png";
                Title = "Crear";
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
            set => SetProperty(ref _actividad, value);
        }
        public NameDTO Upa
        {
            get => _upa;
            set => SetProperty(ref _upa, value);
        }

        public NameDTO Modulo
        {
            get => _modulo;
            set => SetProperty(ref _modulo, value);
        }


        public InfoComponenteDTO InfoComponente
        {
            get => _infoComponente;
            set => SetProperty(ref _infoComponente, value);
        }
        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
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
        public string JsonEstadoComponente
        {
            get => _jsonEstadoComponente;
            set => SetProperty(ref _jsonEstadoComponente, value);
        }
        private async void LoadLists(Guid id)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
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
                        $"detalle-upas-actividades/listar-actividades-upa-usuario?UpaId={Guid.Empty}&UsuarioId={TokenUser.User.Id}", TokenUser.Token);
                        Actividades = new ObservableCollection<NameDTO>((List<NameDTO>)actividades.Data);
                    }
                    if (id != Guid.Empty)
                    {
                        var infoComponente = await _apiService.GetByIdAsyncWithToken(URL_API,
                            $"componentes/{id}", TokenUser.Token);
                        if (infoComponente.IsExito)
                        {
                            InfoComponente = ParsearData<InfoComponenteDTO>(infoComponente);
                            var modulo = Modulos.Where(x => x.Id == InfoComponente.Modulo.Id).FirstOrDefault();
                            var upa = Upas != null ? Upas.Where(x => x.Id == InfoComponente.Upa.Id).FirstOrDefault() : null;
                            var actividad = Actividades.Where(x => x.Id == InfoComponente.Actividad.Id).FirstOrDefault();

                            if (modulo != null && actividad != null)
                            {
                                Upa = upa;
                                Modulo = modulo;
                                Actividad = actividad;
                            }
                            JsonEstadoComponente = JsonConvert.SerializeObject(InfoComponente.EstadoComponente, Formatting.Indented);
                        }
                    }
                }
                else
                {
                    AlertNoInternetConnection();
                    await _navigationService.GoBackAsync();
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
            var parameters = new NavigationParameters
            {
                {
                    "estadoComponente",
                    new InfoEstadoComponenteModel
                    {
                        Estado = InfoComponente.Id != Guid.Empty ? InfoComponente.EstadoComponente : null,
                        IsEdit = true
                    }
                }
            };
            await _navigationService.NavigateAsync($"{nameof(InfoEstadoPopupPage)}", parameters);
        }

        private async void CreateOrEditClicked(object obj)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Guardando...");
                if (this.AreFieldsValid())
                {
                    if (_apiService.CheckConnection())
                    {
                        if (InfoComponente.Id == Guid.Empty)
                        {
                            var createRequest = new CreateComponenteRequest
                            {
                                Nombre = InfoComponente.Nombre,
                                ActividadId = Actividad.Id,
                                ModuloComponenteId = Modulo.Id,
                                TipoEstadoComponente = InfoComponente.EstadoComponente
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
                                await _navigationService.GoBackAsync(parameters);
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
                                UpaId = Upa.Id,
                                ModuloComponenteId = Modulo.Id
                            };
                            ResponseDTO respuesta = await _apiService.PutAsyncWithToken(URL_API, "componentes/editar",
                                EditRequest, TokenUser.Token);
                            if (respuesta.IsExito)
                            {
                                AlertSuccess(respuesta.MensajeHttp);
                                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                                await _navigationService.GoBackAsync(parameters);
                            }
                            else
                            {
                                AlertWarning(respuesta.MensajeHttp);
                            }
                        }
                    }
                    else
                    {
                        AlertNoInternetConnection();
                    }
                }
                else
                {
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage(@$"Error: Todos los campos son obligatorios.", Constants.ICON_WARNING));
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
            bool isEstadoValid = InfoComponente.EstadoComponente != null;

            if (TokenUser.User.RolId == (int)TipoRol.Administrador)
            {
                isUpaValid = true;
            }
            return isNameValid && isEstadoValid && isUpaValid && isModuloValid && isActividadValid;
        }
    }
}