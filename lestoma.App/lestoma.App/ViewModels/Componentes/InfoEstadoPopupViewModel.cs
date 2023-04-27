using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Listados;
using Newtonsoft.Json;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class InfoEstadoPopupViewModel : BaseViewModel
    {
        #region attributes
        private ObservableCollection<EstadoComponenteDTO> _estados;
        private EstadoComponenteDTO _estadoComponente;
        private bool _isSuperAdmin;
        #endregion

        #region ctor y OnNavigatedTo
        public InfoEstadoPopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _estadoComponente = new EstadoComponenteDTO();
            _estados = new ObservableCollection<EstadoComponenteDTO>();
            SaveEstadoCommand = new Command(SaveClicked);
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
                else
                {
                    IsSuperAdmin = false;
                }
                LoadEstadosComponente(data.Estado);
            }
        }
        #endregion

        #region properties
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
        #endregion

        #region methods
        private async void LoadEstadosComponente(EstadoComponenteDTO estadoComponente)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                await Task.Delay(1000);
                ListadoEstadoComponente estados = new ListadoEstadoComponente();
                Estados = new ObservableCollection<EstadoComponenteDTO>(estados.Listado);
                if (Estados.Count > 0)
                {
                    if (estadoComponente != null)
                    {
                        var data = Estados.Where(x => x.Id == estadoComponente.Id).FirstOrDefault();
                        if (data != null)
                        {
                            EstadoComponente = data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            {
                UserDialogs.Instance.HideLoading();
            }

        }
        private async void SaveClicked()
        {
            try
            {
                if (_estadoComponente == null)
                {
                    AlertWarning("El estado es requerido.");
                    return;
                }

                MovilSettings.EstadoComponente = JsonConvert.SerializeObject(EstadoComponente);
                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                await NavigationService.GoBackAsync(parameters);
            }
            catch (Exception ex)
            {
                AlertError(ex.Message);
            }
        }
        #endregion
    }
}