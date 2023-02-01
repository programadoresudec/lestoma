using Acr.UserDialogs;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class CreateOrEditDetalleUpaActividadViewModel : BaseViewModel
    {
        private IApiService _apiService;
        private ObservableCollection<UserDTO> _users;
        private UserDTO _user;
        private ObservableCollection<NameDTO> _upas;
        private NameDTO _upa;
        private bool _isvisible;
        private bool _isEdit = true;
        private ObservableCollection<NameDTO> _actividades;
        public ObservableCollection<NameDTO> _ActividadesAdd;
        private DetalleUpaActividadDTO _detalleUpaActividad;
        public CreateOrEditDetalleUpaActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
            _users = new ObservableCollection<UserDTO>();
            _upas = new ObservableCollection<NameDTO>();
            _actividades = new ObservableCollection<NameDTO>();
            _ActividadesAdd = new ObservableCollection<NameDTO>();
            _user = new UserDTO();
            _upa = new NameDTO();
            CreateOrEditCommand = new Command(CreateOrEditClicked);
            ItemSelectCommand = new Command((param) => ItemSelectClicked(param));
            ItemRemoveCommand = new Command((param) => ItemRemoveClicked(param));
        }

        private void ItemSelectClicked(object param)
        {
            try
            {
                var addedActividad = param;

                var json = JsonConvert.SerializeObject(addedActividad);
                json = json.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                var actividad = JsonConvert.DeserializeObject<NameDTO>(json);

                if (actividad != null)
                {
                    ActividadesAdd.Add(actividad);
                    var obj = Actividades.Where(x => x.Id == actividad.Id).FirstOrDefault();
                    Actividades.Remove(obj);
                    IsVisibleActividades = true;
                    Page currentPage = Application.Current.MainPage;
                    var comboBox = currentPage.FindByName<Syncfusion.XForms.ComboBox.SfComboBox>("comboBoxActividades");
                    var entry = currentPage.FindByName<Entry>("PassEntry");

                    comboBox.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void ItemRemoveClicked(object param)
        {
            try
            //
            {
                var sfChip = param as Syncfusion.XForms.Buttons.SfChip;
                if (sfChip != null)
                {
                    var dataContext = GetInternalProperty(typeof(Syncfusion.XForms.Buttons.SfChip), sfChip, "DataContext");
                    var removeActividad = dataContext;
                    var json = JsonConvert.SerializeObject(removeActividad);
                    json = json.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    var actividad = JsonConvert.DeserializeObject<NameDTO>(json);
                    if (actividad != null)
                    {
                        var obj = ActividadesAdd.Where(x => x.Id == actividad.Id).FirstOrDefault();
                        ActividadesAdd.Remove(obj);
                        Actividades.Add(actividad);
                        if (ActividadesAdd.Count == 0)
                        {
                            IsVisibleActividades = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
            get => _ActividadesAdd;
            set => SetProperty(ref _ActividadesAdd, value);
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
        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("detalleUpaActividad"))
            {
                DetalleUpaActividad = parameters.GetValue<DetalleUpaActividadDTO>("detalleUpaActividad");
                Title = "Editar Detalle";
                CargarObjetos(DetalleUpaActividad);
            }
            else
            {
                Title = "Crear Detalle";
                CargarObjetos();
            }
        }

        private async void CargarObjetos(DetalleUpaActividadDTO detalleUpaActividad = null)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                           "actividades/listar-nombres", TokenUser.Token);

                    ResponseDTO response1 = await _apiService.GetListAsyncWithToken<List<UserDTO>>(URL_API,
                        "usuarios/activos", TokenUser.Token);

                    ResponseDTO response2 = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                        "upas/listar-nombres", TokenUser.Token);

                    var listadoActividades = (List<NameDTO>)response.Data;
                    var listadoUsuarios = (List<UserDTO>)response1.Data;
                    var listadoUpas = (List<NameDTO>)response2.Data;

                    Upas = new ObservableCollection<NameDTO>(listadoUpas);
                    Usuarios = new ObservableCollection<UserDTO>(listadoUsuarios);
                    if (detalleUpaActividad == null)
                    {
                        Actividades = new ObservableCollection<NameDTO>(listadoActividades);
                    }
                    else
                    {
                        IsVisibleActividades = true;
                        IsEdit = false;
                        Upa = Upas.Where(x => x.Id == detalleUpaActividad.UpaId).FirstOrDefault();
                        User = Usuarios.Where(x => x.Id == detalleUpaActividad.UsuarioId).FirstOrDefault();
                        ResponseDTO listaActividadesxUser = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                          $"detalle-upas-actividades/lista-actividades-by-upa-usuario?UpaId=" +
                          $"{Upa.Id}&UsuarioId={User.Id}", TokenUser.Token);
                        if (listaActividadesxUser.IsExito)
                        {
                            var listado = (List<NameDTO>)listaActividadesxUser.Data;
                            Actividades = new ObservableCollection<NameDTO>(listadoActividades);
                            foreach (var item in listado)
                            {
                                var actividad = Actividades.Where(x => x.Id == item.Id).FirstOrDefault();
                                Actividades.Remove(actividad);
                            }
                            ActividadesAdd = new ObservableCollection<NameDTO>(listado);
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


        private void CreateOrEditClicked(object obj)
        {

            if (_apiService.CheckConnection())
            {
                if (DetalleUpaActividad == null)
                {
                    Crear();
                }
                else
                {
                    editar();
                }

            }
            else
            {
                AlertNoInternetConnection();
            }
        }

        private async void Crear()
        {
            try
            {
                var detalle = new CrearDetalleUpaActividadRequest
                {
                    UpaId = Upa.Id,
                    UsuarioId = User.Id
                };
                foreach (var item in ActividadesAdd)
                {
                    detalle.Actividades.Add(new ActividadRequest
                    {
                        Id = item.Id,
                        Nombre = item.Nombre
                    });
                }

                var response = await _apiService.PostAsyncWithToken(URL_API, "detalle-upas-actividades/crear", detalle, TokenUser.Token);
                if (response.IsExito)
                {
                    AlertSuccess(response.MensajeHttp);
                    var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                    await _navigationService.GoBackAsync(parameters);
                }
                else
                {
                    AlertWarning(response.MensajeHttp);
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

        private async void editar()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Guardando");
                var detalle = new CrearDetalleUpaActividadRequest
                {
                    UpaId = Upa.Id,
                    UsuarioId = User.Id
                };
                foreach (var item in ActividadesAdd)
                {
                    detalle.Actividades.Add(new ActividadRequest
                    {
                        Id = item.Id,
                        Nombre = item.Nombre
                    });
                }

                var response = await _apiService.PutAsyncWithToken(URL_API, "detalle-upas-actividades/editar", detalle, TokenUser.Token);
                if (response.IsExito)
                {
                    AlertSuccess(response.MensajeHttp);
                    var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                    await _navigationService.GoBackAsync(parameters);
                }
                else
                {
                    AlertWarning(response.MensajeHttp);
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
}