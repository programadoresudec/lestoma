using lestoma.App.Views.Modulos;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class ModuloUpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<NameDTO> _modulos;
        public ModuloUpaViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
            _modulos = new ObservableCollection<NameDTO>();
            seeComponentCommand = new Command<object>(ModuloSelected, CanNavigate);
            LoadModulos();
        }
        private bool CanNavigate(object arg)
        {
            return true;
        }


        public ObservableCollection<NameDTO> Modulos
        {
            get => _modulos;
            set => SetProperty(ref _modulos, value);
        }

        public Command seeComponentCommand { get; set; }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                LoadModulos();
            }
        }
        private async void ModuloSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var modulo = lista.ItemData as NameDTO;

            if (modulo == null)
                return;

            var parameters = new NavigationParameters
            {
                { "ModuloId", modulo.Id }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditModuloPage), parameters);

        }
        private void LoadModulos()
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
                Modulos = new ObservableCollection<NameDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                    $"laboratorio-lestoma/listar-modulos-upa-actividad-por-usuario", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<NameDTO>)response.Data;
                    if (listado.Count > 0)
                    {
                        Modulos = new ObservableCollection<NameDTO>(listado);
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

