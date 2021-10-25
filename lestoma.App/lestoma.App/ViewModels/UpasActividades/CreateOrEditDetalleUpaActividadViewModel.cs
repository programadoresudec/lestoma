using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class CreateOrEditDetalleUpaActividadViewModel : BaseViewModel
    {
        private INavigationService _navigationService;
        private IApiService _apiService;

        private ObservableCollection<UserDTO> _users;
        private ObservableCollection<NameDTO> _upas;
        private ObservableCollection<NameDTO> _actividades;

        private DetalleUpaActividadDTO _detalleUpaActividad;
        public CreateOrEditDetalleUpaActividadViewModel(INavigationService navigationService, IApiService apiService) :
        base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _users = new ObservableCollection<UserDTO>();
            _upas = new ObservableCollection<NameDTO>();
            _actividades = new ObservableCollection<NameDTO>();
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
            Response listadoActividades = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL,
            "actividades/listado", TokenUser.Token);

            Response listadoUsuarios = await _apiService.GetListAsyncWithToken<List<UserDTO>>(URL,
            "usuarios/listado-nombres", TokenUser.Token);

            Response listadoUpas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL,
          "usuarios/listado-nombres", TokenUser.Token);

            if (detalleUpaActividad == null)
            {

            }
            else
            {

            }
        }

        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
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

    }
}
