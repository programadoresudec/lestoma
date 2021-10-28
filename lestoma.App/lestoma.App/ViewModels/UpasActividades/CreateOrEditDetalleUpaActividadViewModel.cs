using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class CreateOrEditDetalleUpaActividadViewModel : BaseViewModel
    {
        private INavigationService _navigationService;
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
            _navigationService = navigationService;
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
            {
                var actividad = param as NameDTO;
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
                Title = "Editar";
                CargarObjetos(DetalleUpaActividad);
            }
            else
            {
                Title = "Crear";
                CargarObjetos();
            }
        }

        private async void CargarObjetos(DetalleUpaActividadDTO detalleUpaActividad = null)
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    CrossToastPopUp.Current.ShowToastError("Error no hay conexión a internet.");
                    return;
                }

                Response response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL,
                    "actividades/listado-nombres", TokenUser.Token);
                var listadoActividades = (List<NameDTO>)response.Data;

                Response response1 = await _apiService.GetListAsyncWithToken<List<UserDTO>>(URL,
                    "usuarios/listado-nombres", TokenUser.Token);
                var listadoUsuarios = (List<UserDTO>)response1.Data;

                Response response2 = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL,
                    "upas/listado-nombres", TokenUser.Token);
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
                    Upa = listadoUpas.Where(x => x.Id == detalleUpaActividad.UpaId).FirstOrDefault();
                    User = listadoUsuarios.Where(x => x.Id == detalleUpaActividad.UsuarioId).FirstOrDefault();

                    foreach (var item in detalleUpaActividad.Actividades)
                    {
                        var actividad = listadoActividades.Where(x => x.Id == item.Id).FirstOrDefault();
                        listadoActividades.Remove(actividad);
                    }
                    Actividades = new ObservableCollection<NameDTO>(listadoActividades);
                    foreach (var item in detalleUpaActividad.Actividades)
                    {
                        ActividadesAdd.Add(new NameDTO
                        {
                            Id = item.Id,
                            Nombre = item.Nombre
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        private async void CreateOrEditClicked(object obj)
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    CrossToastPopUp.Current.ShowToastError("Error no hay conexión a internet.");
                    return;
                }


                await _navigationService.NavigateAsync(nameof(LoadingPopupPage));

                if (DetalleUpaActividad == null)
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

                    var response = await _apiService.PostAsyncWithToken(URL, "detalle-upas-actividades/crear", detalle, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        CrossToastPopUp.Current.ShowToastError(response.Mensaje);
                        return;
                    }

                    CrossToastPopUp.Current.ShowToastSuccess(response.Mensaje);
                    await Task.Delay(2000);
                    await _navigationService.GoBackAsync();
                }
                else
                {
                    foreach (var item in ActividadesAdd)
                    {
                        foreach (var item1 in DetalleUpaActividad.Actividades)
                        {
                            if (item.Id != item1.Id)
                            {
                                DetalleUpaActividad.Actividades.Add(new ActividadRequest
                                {
                                    Id = item.Id,
                                    Nombre = item.Nombre
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                await _navigationService.ClearPopupStackAsync();
            }

        }
    }
}