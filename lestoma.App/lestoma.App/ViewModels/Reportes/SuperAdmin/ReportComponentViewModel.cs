using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.App.Views.Reportes;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes.SuperAdmin
{
    class ReportComponentViewModel : BaseViewModel
    {
        #region attributes
        private readonly IApiService _apiService;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private ObservableCollection<ComponenteCheckModel> _Components;
        private bool _isSuperAdmin;
        private bool _isEnabled;
        private FiltroFechaModel _filtroFecha;
        private NameDTO _modulo;
        private ObservableCollection<NameDTO> _modulos;
        private ObservableCollection<ComponenteCheckModel> _componentsAdd;
        private ComponenteCheckModel _componentSelected;
        private NameArchivoDTO _tipoArchivo;
        private ObservableCollection<NameArchivoDTO> _tipoArchivos;
        #endregion

        #region constructor

        public ReportComponentViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            Title = "Reporte por fechas y componentes";
            _Components = new ObservableCollection<ComponenteCheckModel>();
            _componentsAdd = new ObservableCollection<ComponenteCheckModel>();
            SendCommand = new Command(GenerateReportClicked);
            ListarUpas();
            ListarModulos();
            ListarTiposFormato();
        }
        #endregion

        #region properties
        public FiltroFechaModel FiltroFecha
        {
            get => _filtroFecha;
            set => SetProperty(ref _filtroFecha, value);
        }
        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                if (_modulo == null)
                {
                    ListarComponentes(_upa.Id, Guid.Empty);
                }
                else
                {
                    ListarComponentes(_upa.Id, _modulo.Id);
                }
            }
        }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }
        public ObservableCollection<ComponenteCheckModel> Components
        {
            get => _Components;
            set => SetProperty(ref _Components, value);
        }
        public ComponenteCheckModel ComponentSelected
        {
            get => _componentSelected;
            set
            {
                SetProperty(ref _componentSelected, value);
                AddComponent(_componentSelected);
            }
        }

        public ObservableCollection<ComponenteCheckModel> ComponentsAdd
        {
            get => _componentsAdd;
            set => SetProperty(ref _componentsAdd, value);
        }
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
        public ObservableCollection<NameArchivoDTO> TipoArchivos
        {
            get => _tipoArchivos;
            set => SetProperty(ref _tipoArchivos, value);
        }

        public NameArchivoDTO TipoArchivo
        {
            get => _tipoArchivo;
            set => SetProperty(ref _tipoArchivo, value);
        }

        public ObservableCollection<NameDTO> Modulos
        {
            get => _modulos;

            set => SetProperty(ref _modulos, value);
        }

        public NameDTO Modulo
        {
            get => _modulo;
            set
            {
                SetProperty(ref _modulo, value);
                if (_upa == null)
                {
                    ListarComponentes(Guid.Empty, _modulo.Id);
                }
                else
                {
                    ListarComponentes(_upa.Id, _modulo.Id);
                }
            }
        }
        public Command ItemRemoveCommand { get; }

        public Command NavigatePopupFilterCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await NavigationService.NavigateAsync(nameof(FilterDatePopupPage));
                });
            }
        }
        public Command SendCommand { get; set; }
        #endregion

        #region Methods

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("filtroFecha"))
            {
                FiltroFecha = parameters.GetValue<FiltroFechaModel>("filtroFecha");
            }
        }
        private async void ListarModulos()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                    $"laboratorio-lestoma/listar-modulos-upa-actividad-por-usuario", TokenUser.Token);
                    if (response.IsExito)
                    {
                        var listado = (List<NameDTO>)response.Data;
                        if (listado.Count > 0)
                        {
                            Modulos = new ObservableCollection<NameDTO>(listado);
                            Modulos.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todos" });
                        }
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

        private void ListarTiposFormato()
        {
            _tipoArchivos = new ObservableCollection<NameArchivoDTO>()
            {
                new NameArchivoDTO()
                {
                    Id = (int)GrupoTipoArchivo.PDF,
                    Nombre = GrupoTipoArchivo.PDF.ToString(),
                },
                 new NameArchivoDTO()
                {
                    Id = (int) GrupoTipoArchivo.CSV,
                    Nombre = GrupoTipoArchivo.CSV.ToString(),
                },
                new NameArchivoDTO()
                {
                    Id = (int) GrupoTipoArchivo.EXCEL,
                    Nombre = GrupoTipoArchivo.EXCEL.ToString(),
                }
            };
        }
        private void AddComponent(ComponenteCheckModel componentSelected)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Agregando...");
                var checkAll = _componentsAdd.Any(x => x.Id == Guid.Empty);
                var existe = _componentsAdd.Any(x => x.Id == componentSelected.Id);
                var count = _componentsAdd.Count(x => x.Id != Guid.Empty);
                if (!existe && !checkAll)
                {
                    if (componentSelected.Id == Guid.Empty && count > 0)
                        return;
                    ComponentsAdd.Add(componentSelected);
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
        private async void ListarUpas()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                    ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                    Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                    Upas.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                    if (!IsSuperAdmin)
                    {
                        ResponseDTO response = await _apiService.GetAsyncWithToken(URL_API, "usuarios/upa-asignada", TokenUser.Token);
                        if (response.IsExito)
                        {
                            var upa = ParsearData<NameDTO>(response);
                            var selected = Upas.Where(x => x.Id == upa.Id).FirstOrDefault();
                            Upa = selected;
                            ListarComponentes(upa.Id, Guid.Empty);
                        }
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
        private async void GenerateReportClicked(object obj)
        {
            try
            {
                if (!this.Validations())
                {
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage("Todos los campos son requeridos.", Constants.ICON_WARNING));
                    return;
                }

                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                UserDialogs.Instance.ShowLoading("Enviando...");
                ReportComponentFilterRequest reportComponentFilterRequest = new ReportComponentFilterRequest
                {
                    ComponentesId = _componentsAdd.Select(x => x.Id).ToList(),
                    Filtro = new ReportFilterRequest
                    {
                        FechaInicial = _filtroFecha.FechaInicio,
                        FechaFinal = _filtroFecha.FechaFin,
                        TipoFormato = GetFormatFile(TipoArchivo.Id),
                        UpaId = Upa.Id
                    }
                };

                var response = await _apiService.PostAsyncWithToken(URL_API, "reports-laboratory/by-components", reportComponentFilterRequest, TokenUser.Token);
                await PopupNavigation.Instance.PushAsync(new MessagePopupPage(response.MensajeHttp));
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
        private GrupoTipoArchivo GetFormatFile(int idFormato)
        {
            switch (idFormato)
            {
                case (int)GrupoTipoArchivo.PDF:
                    return GrupoTipoArchivo.PDF;
                case (int)GrupoTipoArchivo.CSV:
                    return GrupoTipoArchivo.CSV;
                case (int)GrupoTipoArchivo.EXCEL:
                    return GrupoTipoArchivo.EXCEL;
                default:
                    return GrupoTipoArchivo.PDF;
            }
        }
        private bool Validations()
        {
            bool isFiltroFechaValid = _filtroFecha != null;
            bool isUpaValid = Upa != null;
            bool istipoArchivoValid = TipoArchivo != null;
            bool isComponentValid = _componentsAdd.Count > 0;

            if (TokenUser.User.RolId == (int)TipoRol.Administrador)
                isUpaValid = true;

            return isFiltroFechaValid && isUpaValid && istipoArchivoValid && isComponentValid;
        }

        private async void ListarComponentes(Guid upaId, Guid moduloId)
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                    Components.Clear();
                    ComponentsAdd.Clear();
                    var FilterRequest = new UpaModuleFilterRequest
                    {
                        UpaId = upaId,
                        ModuloId = moduloId,
                    };
                    string queryString = Reutilizables.GenerateQueryString(FilterRequest);

                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ComponenteCheckModel>>(URL_API,
                        $"componentes/listar-nombres-por-upa-modulo/{queryString}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        Components = new ObservableCollection<ComponenteCheckModel>((List<ComponenteCheckModel>)response.Data);
                        if (Components.Count == 0)
                        {
                            AlertWarning("No hay componentes.");
                            return;
                        }

                        IsEnabled = true;
                        if (_modulo == null)
                        {
                            Components.Insert(0, new ComponenteCheckModel { Id = Guid.Empty, Nombre = "Todos" });
                        }
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


        #endregion
    }
}
