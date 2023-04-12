using lestoma.App.Models;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Listados;
using Newtonsoft.Json;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class InfoEstadoPopupViewModel : BaseViewModel
    {
        private ObservableCollection<EstadoComponenteDTO> _estados;
        private EstadoComponenteDTO _estadoComponente;
        private bool _isSuperAdmin;

        public InfoEstadoPopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _estadoComponente = new EstadoComponenteDTO();
            _estados = new ObservableCollection<EstadoComponenteDTO>();
            SaveEstadoCommand = new Command(SaveClicked);
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
        }

        private async void SaveClicked()
        {
            if (EstadoComponente.Id != Guid.Empty)
            {
                MovilSettings.EstadoComponente = JsonConvert.SerializeObject(EstadoComponente);
                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                await _navigationService.GoBackAsync(parameters);
            }
            else
            {
                AlertWarning("El estado es requerido.");
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("estadoComponente"))
            {
                 var data = parameters.GetValue<InfoEstadoComponenteModel>("estadoComponente");
                if (data.IsCreated)
                {
                    IsSuperAdmin = true;
                }
                LoadEstadosComponente(data.Estado);
            }
        }

        public ObservableCollection<EstadoComponenteDTO> Estados
        {
            get => _estados;
            set => SetProperty(ref _estados, value);
        }

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }

        public EstadoComponenteDTO EstadoComponente
        {
            get => _estadoComponente;
            set => SetProperty(ref _estadoComponente, value);
        }

        public Command SaveEstadoCommand { get; set; }

        private void LoadEstadosComponente(EstadoComponenteDTO estadoComponente)
        {
            ListadoEstadoComponente estados = new ListadoEstadoComponente();
            Estados = new ObservableCollection<EstadoComponenteDTO>(estados.Listado);

            if (Estados.Count > 0)
            {
                var data = Estados.Where(x => x.Id == estadoComponente.Id).FirstOrDefault();
                if (data != null)
                {
                    EstadoComponente = data;
                }
            }
        }
    }
}