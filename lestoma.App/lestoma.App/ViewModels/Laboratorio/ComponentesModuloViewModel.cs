using lestoma.App.Views.Laboratorio;
using lestoma.App.Views.Modulos;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class ComponentesModuloViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<ComponentePorModuloDTO> _componentes;
        private Guid _moduloId;
        public ComponentesModuloViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
            RedirectionTramaCommand = new Command<object>(ComponentSelected, CanNavigate);
            Title = "Seleccione uno o mas componentes";

        }
        private bool CanNavigate(object arg)
        {
            return true;
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("ModuloId"))
            {
                _moduloId = parameters.GetValue<Guid>("ModuloId");
                LoadComponents();
            }

        }

        public ObservableCollection<ComponentePorModuloDTO> Componentes
        {
            get => _componentes;
            set => SetProperty(ref _componentes, value);
        }

        public Command RedirectionTramaCommand { get; set; }

        private async void ComponentSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var componente = lista.ItemData as ComponentePorModuloDTO;
            if (componente == null)
                return;

            var parameters = new NavigationParameters
            {
                { "ModuloId", componente.Id }
            };
            if (EnumConfig.GetDescription(TipoEstadoComponente.Lectura).Equals(componente.EstadoComponente.TipoEstado))
            {
                await _navigationService.NavigateAsync(nameof(LecturaSensorPage), parameters);
            }
            if (EnumConfig.GetDescription(TipoEstadoComponente.OnOff).Equals(componente.EstadoComponente.TipoEstado))
            {
                await _navigationService.NavigateAsync(nameof(EstadoActuadorPage), parameters);
            }
            else
            {
                await _navigationService.NavigateAsync(nameof(SetPointPage), parameters);
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
    }
}