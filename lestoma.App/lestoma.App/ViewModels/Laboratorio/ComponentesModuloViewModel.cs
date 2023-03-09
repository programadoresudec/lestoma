using Acr.UserDialogs;
using lestoma.App.Views.Laboratorio;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class ComponentesModuloViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<ComponentePorModuloDTO> _componentes;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private ObservableCollection<NameProtocoloDTO> _protocolos;
        private NameProtocoloDTO _protocolo;
        private bool _isSuperAdmin;
        private Guid _moduloId;
        private int? _esclavo;

        public ComponentesModuloViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            RedirectionTramaCommand = new Command<object>(ComponentSelected, CanNavigate);
            Title = "Seleccione un componente";
            LoadUpas();
            Bytes = LoadBytes();
        }


        #region Properties
        public Command RedirectionTramaCommand { get; set; }

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;

            set => SetProperty(ref _upas, value);
        }


        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                ListarProtocolos(_upa.Id);
            }
        }
        public ObservableCollection<NameProtocoloDTO> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }


        public NameProtocoloDTO Protocolo
        {
            get => _protocolo;
            set => SetProperty(ref _protocolo, value);
        }

        public ObservableCollection<ComponentePorModuloDTO> Componentes
        {
            get => _componentes;
            set => SetProperty(ref _componentes, value);
        }
        public List<int> Bytes { get; set; }

        public int? Esclavo
        {
            get => _esclavo;
            set => SetProperty(ref _esclavo, value);
        }

        private List<int> LoadBytes()
        {
            return Enumerable.Range(0, 256).ToList();
        }

        #endregion

        #region Methods
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("ModuloId"))
            {
                _moduloId = parameters.GetValue<Guid>("ModuloId");
                LoadComponents();
            }
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }
        private async void LoadUpas()
        {
            try
            {
                if (_isSuperAdmin)
                {
                    // consume service en la nube
                    if (_apiService.CheckConnection())
                    {
                        ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                        if (response.IsExito)
                        {
                            Upas = new ObservableCollection<NameDTO>((List<NameDTO>)response.Data);
                        }
                    }
                    // consume service en la bd del dispositivo movil
                    else
                    {

                    }
                }
                else
                {
                    ListarProtocolos(Guid.Empty);
                }

            }
            catch (Exception ex)
            {
                SeeError(ex);
            }

        }
        private void LoadComponents()
        {
            if (_apiService.CheckConnection())
            {
                ConsumoService();
            }
            else
            {
                ConsumoServiceLocal();
            }
        }

        private async void ListarProtocolos(Guid UpaId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameProtocoloDTO>>(URL_API, $"upas/listar-nombres-protocolo/{UpaId}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        Protocolos = new ObservableCollection<NameProtocoloDTO>((List<NameProtocoloDTO>)response.Data);
                    }
                }
                else
                {

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

        private void ConsumoServiceLocal()
        {
            throw new NotImplementedException();
        }


        private async void ConsumoService()
        {
            try
            {
                IsBusy = true;
                Componentes = new ObservableCollection<ComponentePorModuloDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ComponentePorModuloDTO>>(URL_API,
                    $"laboratorio-lestoma/listar-componentes-modulo/{_moduloId}", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<ComponentePorModuloDTO>)response.Data;
                    if (listado.Count > 0)
                    {
                        Componentes = new ObservableCollection<ComponentePorModuloDTO>(listado);
                    }
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
                IsBusy = false;
            }

        }
        private async void ComponentSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var componente = lista.ItemData as ComponentePorModuloDTO;
            if (componente == null)
                return;

            

            var request = new TramaComponenteRequest
            {
                NombreComponente = componente.Nombre,
                TramaOchoBytes = new List<byte>()
                {
                    Protocolo.PrimerByteTrama,
                    (byte)Esclavo.Value,
                    componente.EstadoComponente.ByteDecimalFuncion,
                    componente.DireccionRegistro,
                    0,
                    0,
                    0,
                    0
                }
            };

            var parameters = new NavigationParameters
            {
                { "tramaComponente", request }
            };

            if (EnumConfig.GetDescription(TipoEstadoComponente.Lectura).Equals(componente.EstadoComponente.TipoEstado))
            {
                await _navigationService.NavigateAsync(nameof(LecturaSensorPage), parameters);
            }
            else if (EnumConfig.GetDescription(TipoEstadoComponente.OnOff).Equals(componente.EstadoComponente.TipoEstado))
            {
                await _navigationService.NavigateAsync(nameof(EstadoActuadorPage), parameters);
            }
            else
            {
                await _navigationService.NavigateAsync(nameof(SetPointPage), parameters);
            }
        }

        #endregion
    }
}