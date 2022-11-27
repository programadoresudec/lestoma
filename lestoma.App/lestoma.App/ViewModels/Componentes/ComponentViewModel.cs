using lestoma.App.Views.Componentes;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class ComponentViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<ComponentesDTO> _componentes;
        public ComponentViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;

            EditCommand = new Command<object>(ComponentSelected, CanNavigate);
            LoadComponents();
        }


        private bool CanNavigate(object arg)
        {
            return true;
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("refresh"))
            {
                ConsumoService(true);
            }
        }

        public ObservableCollection<ComponentesDTO> Componentes
        {
            get => _componentes;
            set => SetProperty(ref _componentes, value);
        }
        public ModuloDTO ItemDelete { get; set; }

        public Command EditCommand { get; set; }
        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(CreateOrEditComponentPage), null, useModalNavigation: true, true);
                });
            }
        }

        private async void ComponentSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var componente = lista.ItemData as ListadoComponenteDTO;

            if (componente == null)
                return;

            var salida = componente.Id;

            var parameters = new NavigationParameters
            {
                { "id", salida }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditComponentPage), parameters, useModalNavigation: true, true);

        }
        private void LoadComponents()
        {
            if (_apiService.CheckConnection())
            {
                ConsumoService();
            }
            else
            {
                AlertNoInternetConnection();
            }
        }

        private async void ConsumoService(bool refresh = false)
        {
            IsBusy = true;
            try
            {
                Componentes = new ObservableCollection<ComponentesDTO>();
                ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<ComponentesDTO>(URL_API,
                    $"componentes/paginar", TokenUser.Token);
                if (response.IsExito)
                {
                    var paginador = (Paginador<ComponentesDTO>)response.Data;
                    if (paginador.Datos.Count > 0)
                    {
                        Componentes = new ObservableCollection<ComponentesDTO>(paginador.Datos);
                    }
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
