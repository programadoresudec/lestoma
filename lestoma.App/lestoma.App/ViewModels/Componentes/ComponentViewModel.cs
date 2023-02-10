using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views.Componentes;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ComponentViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador ? true : false;
            _apiService = apiService;
            EditCommand = new Command<object>(ComponentSelected, CanNavigate);
            VerEstadoCommand = new Command<object>(OnSeeStatusSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
            _componentes = new ObservableCollection<ComponenteDTO>();
            LoadComponents();
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

        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(CreateOrEditComponentPage), null);
                });
            }
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }

        private async void ComponentSelected(object objeto)
        {
            var list = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var component = list.ItemData as ComponenteDTO;

            if (component == null)
                return;

            var salida = component.Id;

            var parameters = new NavigationParameters
            {
                { "idComponent", salida }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditComponentPage), parameters);
        }

        private async void DeleteClicked(object obj)
        {
            ComponenteDTO detalle = (ComponenteDTO)obj;
            if (detalle == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API,
                        "componentes", detalle.Id, TokenUser.Token);
                    if (response.IsExito)
                    {
                        AlertSuccess(response.MensajeHttp);
                        ListarComponentesAll();
                    }
                    else
                    {
                        AlertWarning(response.MensajeHttp);
                    }
                }
                else
                {
                    AlertNoInternetConnection();
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
                        new InfoEstadoComponenteModel { Estado = detalle.TipoEstadoComponente, IsEdit = false }
                    }
                };
                await _navigationService.NavigateAsync($"{nameof(InfoEstadoPopupPage)}", parameters);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }

        private void LoadComponents()
        {
            if (_isSuperAdmin == true)
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
            IsBusy = true;
            try
            {
                if (_apiService.CheckConnection())
                {
                    Componentes.Clear();
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<ComponenteDTO>(URL_API,
                        $"componentes/paginar", TokenUser.Token);
                    if (response.IsExito)
                    {
                        var paginador = (Paginador<ComponenteDTO>)response.Data;
                        if (paginador.Datos.Count > 0)
                        {
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
                IsBusy = false;
            }
        }


        private async void ListarComponentesUpaId(Guid id)
        {
            IsBusy = true;
            try
            {
                if (_apiService.CheckConnection())
                {
                    Componentes.Clear();
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<ComponenteDTO>(URL_API,
                        $"componentes/paginar?UpaId={id}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        var paginador = (Paginador<ComponenteDTO>)response.Data;
                        if (paginador.Datos.Count > 0)
                        {
                            Componentes = new ObservableCollection<ComponenteDTO>(paginador.Datos);
                        }
                    }
                    else
                    {
                        AlertWarning(response.MensajeHttp);
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}