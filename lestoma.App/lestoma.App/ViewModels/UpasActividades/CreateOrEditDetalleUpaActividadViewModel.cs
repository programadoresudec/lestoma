using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class CreateOrEditDetalleUpaActividadViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<UserDTO> _users;
        private UserDTO _user;
        private ObservableCollection<NameDTO> _upas;
        private NameDTO _upa;
        private NameDTO _actividad;
        private bool _isvisible;
        private bool _isCreated = true;
        private ObservableCollection<NameDTO> _actividades;
        public ObservableCollection<NameDTO> _actividadesAdd;
        private DetalleUpaActividadDTO _detalleUpaActividad;
        private bool _isEdit;

        public CreateOrEditDetalleUpaActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
            _users = new ObservableCollection<UserDTO>();
            _upas = new ObservableCollection<NameDTO>();
            _actividades = new ObservableCollection<NameDTO>();
            _actividadesAdd = new ObservableCollection<NameDTO>();
            _user = new UserDTO();
            _upa = new NameDTO();
            CreateOrEditCommand = new Command(CreateOrEditClicked);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("detalleUpaActividad"))
            {
                DetalleUpaActividad = parameters.GetValue<DetalleUpaActividadDTO>("detalleUpaActividad");
                Title = "Editar Detalle";
                IsCreated = false;
                IsEdit = true;
                CargarListados(DetalleUpaActividad, true);
            }
            else
            {
                Title = "Crear Detalle";
                CargarListados();
            }
        }

        public Command CreateOrEditCommand { get; set; }
        public Command ItemSelectCommand { get; }
        public Command ItemRemoveCommand { get; }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }
        public ObservableCollection<NameDTO> ActividadesAdd
        {
            get => _actividadesAdd;
            set => SetProperty(ref _actividadesAdd, value);
        }
        public UserDTO User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public NameDTO Upa
        {
            get => _upa;
            set => SetProperty(ref _upa, value);
        }

        public NameDTO Actividad
        {
            get => _actividad;
            set
            {
                SetProperty(ref _actividad, value);
                AddActivity(_actividad);
            }
        }
        public DetalleUpaActividadDTO DetalleUpaActividad
        {
            get => _detalleUpaActividad;
            set => SetProperty(ref _detalleUpaActividad, value);
        }
        public ObservableCollection<UserDTO> Usuarios
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }
        public ObservableCollection<NameDTO> Actividades
        {
            get => _actividades;
            set => SetProperty(ref _actividades, value);

        }
        public bool IsVisibleActividades
        {
            get => _isvisible;
            set => SetProperty(ref _isvisible, value);
        }
        public bool IsCreated
        {
            get => _isCreated;
            set => SetProperty(ref _isCreated, value);
        }
        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        private bool AreFieldsValid()
        {

            bool isUserValid = User != null;
            bool isUpaValid = Upa != null;
            bool isActividadValid = Actividad != null;
            if (IsEdit)
            {
                isActividadValid = true;
            }
            return isUserValid && isUpaValid && isActividadValid;
        }
        private void AddActivity(NameDTO activity)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Agregando...");
                var checkAll = _actividadesAdd.Any(x => x.Id == Guid.Empty);
                var existe = _actividadesAdd.Any(x => x.Id == activity.Id);
                var count = _actividadesAdd.Count(x => x.Id != Guid.Empty);
                if (!existe && !checkAll)
                {
                    if (activity.Id == Guid.Empty && count > 0)
                        return;
                    ActividadesAdd.Add(activity);
                    IsVisibleActividades = true;
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

        private async void CargarListados(DetalleUpaActividadDTO detalleUpaActividad = null, bool IsEdit = false)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    await NavigationService.GoBackAsync();
                }
                ResponseDTO actividades = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "actividades/listar-nombres", TokenUser.Token);
                ResponseDTO usuarios;
                if (!IsEdit)
                {
                    usuarios = await _apiService.GetListAsyncWithToken<List<UserDTO>>(URL_API, "usuarios/activos-sin-upa", TokenUser.Token);
                }
                else
                {
                    usuarios = await _apiService.GetListAsyncWithToken<List<UserDTO>>(URL_API, "usuarios/listar", TokenUser.Token);
                }
                ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                var listadoActividades = (List<NameDTO>)actividades.Data;
                var listadoUsuarios = (List<UserDTO>)usuarios.Data;
                var listadoUpas = (List<NameDTO>)upas.Data;
                Upas = new ObservableCollection<NameDTO>(listadoUpas);
                Usuarios = new ObservableCollection<UserDTO>(listadoUsuarios);
                if (Usuarios.Count == 0)
                {
                    var check = await UserDialogs.Instance.ConfirmAsync("¡Todos los usuarios ya tienen asignada una UPA!",
                          "Información", "Aceptar","");
                    if (check)
                    {
                        await NavigationService.GoBackAsync();
                    }
                    else
                    {
                        await NavigationService.GoBackAsync();
                    }
                }
                if (detalleUpaActividad == null)
                {
                    Actividades = new ObservableCollection<NameDTO>(listadoActividades);
                    Actividades.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                }
                else
                {
                    IsVisibleActividades = true;
                    Upa = Upas.Where(x => x.Id == detalleUpaActividad.UpaId).FirstOrDefault();
                    User = Usuarios.Where(x => x.Id == detalleUpaActividad.UsuarioId).FirstOrDefault();
                    UpaUserFilterRequest upaUserFilterRequest = new UpaUserFilterRequest
                    {
                        UpaId = Upa.Id,
                        UsuarioId = User.Id
                    };
                    string queryString = Reutilizables.GenerateQueryString(upaUserFilterRequest);
                    ResponseDTO listaActividadesxUser = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                      $"detalle-upas-actividades/listar-por-usuario{queryString}", TokenUser.Token);
                    if (listaActividadesxUser.IsExito)
                    {
                        var listado = (List<NameDTO>)listaActividadesxUser.Data;
                        Actividades = new ObservableCollection<NameDTO>(listadoActividades);
                        Actividades.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                        ActividadesAdd = new ObservableCollection<NameDTO>(listado);
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


        private void CreateOrEditClicked(object obj)
        {
            if (DetalleUpaActividad == null)
            {
                Crear();
            }
            else
            {
                Editar();
            }
        }

        private async void Crear()
        {
            try
            {
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
                UserDialogs.Instance.ShowLoading("Guardando...");
                var detalle = new CrearDetalleUpaActividadRequest
                {
                    UpaId = Upa.Id,
                    UsuarioId = User.Id
                };

                bool todas = ActividadesAdd.Any(x => x.Id == Guid.Empty);
                if (todas)
                {
                    foreach (var item in Actividades)
                    {
                        if (item.Id != Guid.Empty)
                        {
                            detalle.Actividades.Add(new ActividadRequest
                            {
                                Id = item.Id,
                                Nombre = item.Nombre
                            });
                        }
                    }
                }
                else
                {
                    foreach (var item in ActividadesAdd)
                    {
                        detalle.Actividades.Add(new ActividadRequest
                        {
                            Id = item.Id,
                            Nombre = item.Nombre
                        });
                    }
                }
                var response = await _apiService.PostAsyncWithToken(URL_API, "detalle-upas-actividades/assign-to-a-user", detalle, TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertWarning(response.MensajeHttp);
                    return;
                }

                AlertSuccess(response.MensajeHttp);
                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                await NavigationService.GoBackAsync(parameters);
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

        private async void Editar()
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                var check = await UserDialogs.Instance.ConfirmAsync("¡Recuerde si no deja actividades eliminará el usuario con la upa asignada!",
                    "Alerta", "Aceptar", "Cancelar");
                if (check)
                {
                    UserDialogs.Instance.ShowLoading("Guardando...");
                    var detalle = new CrearDetalleUpaActividadRequest
                    {
                        UpaId = Upa.Id,
                        UsuarioId = User.Id
                    };
                    bool todas = ActividadesAdd.Any(x => x.Id == Guid.Empty);
                    if (todas)
                    {
                        foreach (var item in Actividades)
                        {
                            if (item.Id != Guid.Empty)
                            {
                                detalle.Actividades.Add(new ActividadRequest
                                {
                                    Id = item.Id,
                                    Nombre = item.Nombre
                                });
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in ActividadesAdd)
                        {
                            detalle.Actividades.Add(new ActividadRequest
                            {
                                Id = item.Id,
                                Nombre = item.Nombre
                            });
                        }
                    }

                    var response = await _apiService.PutAsyncWithToken(URL_API, "detalle-upas-actividades/editar", detalle, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertWarning(response.MensajeHttp);
                        return;
                    }

                    AlertSuccess(response.MensajeHttp);
                    var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                    await NavigationService.GoBackAsync(parameters);
                }
                else
                    return;
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