using lestoma.App.Models;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Listados;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class InfoEstadoPopupViewModel : BaseViewModel
    {

        private ObservableCollection<EstadoComponenteDTO> _estados;
        private EstadoComponenteDTO _estadoComponente;
        private string _error;
        private bool _isVisible = false;
        private bool _isEdit = false;
        public InfoEstadoPopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _estadoComponente = new EstadoComponenteDTO();
            _estados = new ObservableCollection<EstadoComponenteDTO>();
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("estadoComponente"))
            {
                var data = parameters.GetValue<InfoEstadoComponenteModel>("estadoComponente");
                IsEdit = data.IsEdit;
                LoadEstadosComponente(data.Estado);
            }
        }

        public ObservableCollection<EstadoComponenteDTO> Estados
        {
            get => _estados;
            set => SetProperty(ref _estados, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }
        public EstadoComponenteDTO EstadoComponente
        {
            get => _estadoComponente;
            set => SetProperty(ref _estadoComponente, value);
        }

        public Command SaveEstadoCommand { get; set; }

        private void LoadEstadosComponente(EstadoComponenteDTO estadoComponente)
        {
            Estados = new ObservableCollection<EstadoComponenteDTO>(new ListadoEstadoComponente().Listado);
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
