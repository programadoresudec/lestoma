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
        private readonly IApiService _apiService;
        private ObservableCollection<ComponenteDTO> _componentes;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private bool _isSuperAdmin;
        private bool _isNavigating = false;

        public ComponentViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            EditCommand = new Command<object>(ComponentSelected, CanNavigate);
            VerEstadoCommand = new Command<object>(OnSeeStatusSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
            _componentes = new ObservableCollection<ComponenteDTO>();
            LoadComponents();
            MoreComponentsCommand = new Command(LoadMoreComponents);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                ListarComponentesAll();
            }
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

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }

        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                ListarComponentesUpaId(_upa.Id);
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
                ListarComponentesAll();
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


        private async void ListarComponentesUpaId(Guid id)
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
                        UpaId = id
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
                        UpaId = Upa != null ? Upa.Id : Guid.Empty
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
    }
}