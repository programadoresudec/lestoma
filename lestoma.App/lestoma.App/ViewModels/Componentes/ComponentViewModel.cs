using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views.Componentes;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class ComponentViewModel : BaseViewModel
    {
        #region attributes
        private readonly IApiService _apiService;
        private ObservableCollection<ComponenteDTO> _componentes;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private NameDTO _modulo;
        private ObservableCollection<NameDTO> _modulos;
        private bool _isSuperAdmin;
        private bool _isNavigating = false;
        #endregion

        #region ctor y OnNavigatedTo
        public ComponentViewModel(INavigationService navigationService, IApiService apiService) :
           base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            EditCommand = new Command<object>(ComponentSelected, CanNavigate);
            VerEstadoCommand = new Command<object>(OnSeeStatusSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
            _componentes = new ObservableCollection<ComponenteDTO>();
            MoreComponentsCommand = new Command(LoadMoreComponents);
            LoadComponents();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                if (_upa == null && _modulo == null)
                {
                    ListarComponentesAll();
                }
                else if (_upa != null || _modulo != null)
                {
                    ListarComponentesModuloUpaId(_upa != null ? _upa.Id : Guid.Empty, _modulo != null ? _modulo.Id : Guid.Empty);
                }
            }
        }
        #endregion

        #region properties

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }

        public ObservableCollection<ComponenteDTO> Componentes
        {
            get => _componentes;
            set => SetProperty(ref _componentes, value);
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
                if (_modulo == null)
                {
                    ListarComponentesModuloUpaId(_upa.Id, Guid.Empty);
                }
                else
                {
                    ListarComponentesModuloUpaId(_upa.Id, _modulo.Id);
                }
            }
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
                    ListarComponentesModuloUpaId(Guid.Empty, _modulo.Id);
                }
                else
                {
                    ListarComponentesModuloUpaId(_upa.Id, _modulo.Id);
                }
            }
        }
        public ModuloDTO ItemDelete { get; set; }
        public Command EditCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command VerEstadoCommand { get; set; }
        public Command MoreComponentsCommand { get; set; }

        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await NavigationService.NavigateAsync(nameof(CreateOrEditComponentPage), null);
                });
            }
        }
        #endregion

        #region methods
        private bool CanNavigate(object arg)
        {
            return true;
        }

        private async void ComponentSelected(object objeto)
        {
            if (!_isNavigating)
            {
                _isNavigating = true;
                var list = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;

                if (!(list.ItemData is ComponenteDTO component))
                    return;

                var salida = component.Id;

                var parameters = new NavigationParameters
                    {
                        { "idComponent", salida }
                    };
                await NavigationService.NavigateAsync(nameof(CreateOrEditComponentPage), parameters);
                _isNavigating = false;
            }
        }

        private async void DeleteClicked(object obj)
        {
            ComponenteDTO detalle = (ComponenteDTO)obj;
            if (detalle == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API,
                    "componentes", detalle.Id, TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertWarning(response.MensajeHttp);
                    return;
                }
                AlertSuccess(response.MensajeHttp);
                if (_upa == null && _modulo == null)
                {
                    ListarComponentesAll();
                }
                else if (_upa != null || _modulo != null)
                {
                    ListarComponentesModuloUpaId(_upa != null ? _upa.Id : Guid.Empty, _modulo != null ? _modulo.Id : Guid.Empty);
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

        private async void OnSeeStatusSelected(object obj)
        {
            try
            {
                ComponenteDTO detalle = (ComponenteDTO)obj;
                if (detalle == null)
                    return;
                var parameters = new NavigationParameters
                {
                    {
                        "estadoComponente",
                        new InfoEstadoComponenteModel { Estado = detalle.TipoEstadoComponente, IsCreated = false }
                    }
                };
                await NavigationService.NavigateAsync($"{nameof(InfoEstadoPopupPage)}", parameters);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }

        private void LoadComponents()
        {
            if (_isSuperAdmin)
            {
                ListarUpas();
            }
            ListarModulos();
            ListarComponentesAll();
        }

        private async void ListarUpas()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                    Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                    Upas.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }

        }

        private async void ListarModulos()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
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

        }

        private async void ListarComponentesAll()
        {
            try
            {
                Page = 1;
                if (_apiService.CheckConnection())
                {
                    IsBusy = true;
                    Componentes.Clear();
                    var ComponentFilterRequest = new ComponentFilterRequest
                    {
                        Page = this.Page,
                        PageSize = this.PageSize
                    };
                    string querystring = Reutilizables.GenerateQueryString(ComponentFilterRequest);
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<ComponenteDTO>(URL_API, $"componentes/paginar{querystring}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        var paginador = (Paginador<ComponenteDTO>)response.Data;
                        if (paginador.Datos.Any())
                        {
                            IsRefreshing = paginador.HasNextPage;
                            Componentes = new ObservableCollection<ComponenteDTO>(paginador.Datos);
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
                Page++;
                IsBusy = false;
            }
        }


        private async void ListarComponentesModuloUpaId(Guid upaId, Guid moduloId)
        {
            try
            {
                Page = 1;
                if (_apiService.CheckConnection())
                {
                    IsBusy = true;
                    Componentes.Clear();
                    var ComponentFilterRequest = new ComponentFilterRequest
                    {
                        Page = this.Page,
                        PageSize = this.PageSize,
                        UpaId = upaId,
                        ModuloId = moduloId
                    };
                    string querystring = Reutilizables.GenerateQueryString(ComponentFilterRequest);
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<ComponenteDTO>(URL_API, $"componentes/paginar{querystring}", TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertWarning(response.MensajeHttp);
                        return;
                    }
                    var paginador = (Paginador<ComponenteDTO>)response.Data;
                    if (paginador.Datos.Any())
                    {
                        IsRefreshing = paginador.HasNextPage;
                        Componentes = new ObservableCollection<ComponenteDTO>(paginador.Datos);
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                IsBusy = false;
            }
        }

        private async void LoadMoreComponents()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                    var ComponentFilterRequest = new ComponentFilterRequest
                    {
                        Page = this.Page,
                        PageSize = this.PageSize,
                        UpaId = Upa != null ? Upa.Id : Guid.Empty,
                        ModuloId = Modulo != null ? Modulo.Id : Guid.Empty
                    };
                    string querystring = Reutilizables.GenerateQueryString(ComponentFilterRequest);
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<ComponenteDTO>(URL_API, $"componentes/paginar{querystring}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        var paginador = (Paginador<ComponenteDTO>)response.Data;
                        if (paginador.Datos.Any())
                        {
                            foreach (var item in paginador.Datos)
                            {
                                Componentes.Add(item);
                            }
                            IsRefreshing = paginador.HasNextPage;
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
                Page++;
                UserDialogs.Instance.HideLoading();
            }
        }
        #endregion
    }
}