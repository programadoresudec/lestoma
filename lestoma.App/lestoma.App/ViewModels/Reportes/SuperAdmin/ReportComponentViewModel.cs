using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.App.Views.Reportes;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Newtonsoft.Json;
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
        private ObservableCollection<ComponenteCheckModel> _componentsAdd;
        private ComponenteCheckModel _componentSelected;
        private NameDTO _tipoArchivo;
        private ObservableCollection<NameDTO> _tipoArchivos;
        #endregion

        #region constructor

        public ReportComponentViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            Title = "Reporte por rango de fecha y componentes";
            _Components = new ObservableCollection<ComponenteCheckModel>();
            ListarUpas();
            _componentsAdd = new ObservableCollection<ComponenteCheckModel>();
            ItemRemoveCommand = new Command((param) => ItemRemoveClicked(param));
            SendCommand = new Command(GenerateReportClicked);
            _tipoArchivos = new ObservableCollection<NameDTO>()
            {
                new NameDTO()
                {
                    Id = Guid.NewGuid(),
                    Nombre = GrupoTipoArchivo.PDF.ToString(),
                },
                new NameDTO()
                {
                    Id = Guid.NewGuid(),
                    Nombre = GrupoTipoArchivo.EXCEL.ToString(),
                }
            };
        }



        #endregion

        #region properties
        public FiltroFechaModel FiltroFecha
        {
            get => _filtroFecha;
            set => SetProperty(ref _filtroFecha, value);
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
        public ObservableCollection<NameDTO> TipoArchivos
        {
            get => _tipoArchivos;
            set => SetProperty(ref _tipoArchivos, value);
        }

        public NameDTO TipoArchivo
        {
            get => _tipoArchivo;
            set => SetProperty(ref _tipoArchivo, value);
        }
        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                ListarComponentes(_upa.Id);
            }

        }
        public Command ItemRemoveCommand { get; }

        public Command NavigatePopupFilterCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(FilterDatePopupPage));
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
        private void AddComponent(ComponenteCheckModel componentSelected)
        {
            try
            {
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
        }
        private async void ListarUpas()
        {
            if (_isSuperAdmin)
            {
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        UserDialogs.Instance.ShowLoading("Cargando...");
                        ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                        Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                        Upas.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
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
                        TipoFormato = TipoArchivo.Nombre == GrupoTipoArchivo.PDF.ToString() ? GrupoTipoArchivo.PDF : GrupoTipoArchivo.EXCEL,
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

        private async void ListarComponentes(Guid IdUpa)
        {
            IsEnabled = true;
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                    Components.Clear();
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ComponenteCheckModel>>(URL_API, 
                        $"componentes/listar-nombres-por-upa/{IdUpa}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        Components = new ObservableCollection<ComponenteCheckModel>((List<ComponenteCheckModel>)response.Data);
                        Components.Insert(0, new ComponenteCheckModel { Id = Guid.Empty, Nombre = "Todas" });
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

        private void ItemRemoveClicked(object param)
        {
            try
            {
                var sfChip = param as Syncfusion.XForms.Buttons.SfChip;
                if (sfChip != null)
                {
                    var dataContext = GetInternalProperty(typeof(Syncfusion.XForms.Buttons.SfChip), sfChip, "DataContext");
                    var removeActividad = dataContext;
                    var json = JsonConvert.SerializeObject(removeActividad);
                    json = json.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    var componente = JsonConvert.DeserializeObject<ComponenteCheckModel>(json);
                    if (componente != null)
                    {
                        var obj = ComponentsAdd.Where(x => x.Id == componente.Id).FirstOrDefault();
                        ComponentsAdd.Remove(obj);
                        if (ComponentsAdd.Count == 0)
                        {
                            ComponentSelected = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }
        #endregion
    }
}
