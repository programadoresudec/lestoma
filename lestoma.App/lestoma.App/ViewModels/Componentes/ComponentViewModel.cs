using lestoma.App.Views.Componentes;
using lestoma.App.Views.Modulos;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Componentes
{
    public class ComponentViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<ListadoComponenteDTO> _componentes;
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


        public ObservableCollection<ListadoComponenteDTO> Componentes
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
                    await _navigationService.NavigateAsync(nameof(CreateOrEditModuloPage), null, useModalNavigation: true, true);
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

        private void ConsumoService()
        {
            throw new NotImplementedException();
        }
    }
}
