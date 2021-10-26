using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
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
        private ObservableCollection<NameDTO> _actividadesAdd;
        private DetalleUpaActividadDTO _detalleUpaActividad;
        public CreateOrEditDetalleUpaActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _users = new ObservableCollection<UserDTO>();
            _upas = new ObservableCollection<NameDTO>();
            _actividades = new ObservableCollection<NameDTO>();
            _actividadesAdd = new ObservableCollection<NameDTO>();
            _user = new UserDTO();
            _upa = new NameDTO();
            CreateOrEditCommand = new Command(CreateOrEditClicked);
        }


        public Command CreateOrEditCommand { get; set; }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
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
        public ObservableCollection<NameDTO> ActividadesAdd
        {
            get => _actividadesAdd;
            set => SetProperty(ref _actividadesAdd, value);
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

                    foreach (var item in listadoActividades)
                    {
                        foreach (var item2 in detalleUpaActividad.Actividades)
                        {
                            if (item.Id == item2.Id)
                            {
                                listadoActividades.Remove(item);
                            }
                        }
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
                    DetalleUpaActividad = new DetalleUpaActividadDTO
                    {
                        UpaId = Upa.Id,
                        UsuarioId = User.Id
                    };
                    foreach (var item in ActividadesAdd)
                    {
                        DetalleUpaActividad.Actividades.Add(new ActividadRequest
                        {
                            Id = item.Id,
                            Nombre = item.Nombre
                        });
                    }

                    var response = await _apiService.PostAsyncWithToken(URL, "detalle-upas-actividades/crear", DetalleUpaActividad, TokenUser.Token);
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