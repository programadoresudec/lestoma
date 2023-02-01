using Acr.UserDialogs;
using lestoma.App.Views.Modulos;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Modulos
{
    public class ModuloViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<ModuloDTO> _modulos;
        public ModuloViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
            _modulos = new ObservableCollection<ModuloDTO>();
            EditCommand = new Command<object>(UpaSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
            LoadModulos();
        }
        private bool CanNavigate(object arg)
        {
            return true;
        }


        public ObservableCollection<ModuloDTO> Modulos
        {
            get => _modulos;
            set => SetProperty(ref _modulos, value);
        }

        public Command EditCommand { get; set; }
        public Command DeleteCommand { get; set; }
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
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                LoadModulos();
            }
        }
        private async void UpaSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var modulo = lista.ItemData as ModuloDTO;

            if (modulo == null)
                return;

            var salida = new ModuloRequest
            {
                Nombre = modulo.Nombre,
                Id = modulo.Id
            };
            var parameters = new NavigationParameters
            {
                { "modulo", salida }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditModuloPage), parameters, useModalNavigation: true, true);

        }
        private void LoadModulos()
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

        public async void DeleteClicked(object obj)
        {
            ModuloDTO detalle = (ModuloDTO)obj;
            if (detalle == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API,
                        "modulos", detalle.Id, TokenUser.Token);
                    if (response.IsExito)
                    {
                        AlertSuccess(response.MensajeHttp);
                        LoadModulos();
                    }
                    else
                    {
                        AlertWarning(response.MensajeHttp);
                    }

                }
                else
                {
                    AlertNoInternetConnection();
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
        private async void ConsumoService()
        {
            try
            {
                IsBusy = true;
                Modulos = new ObservableCollection<ModuloDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ModuloDTO>>(URL_API,
                    $"modulos/listado", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<ModuloDTO>)response.Data;
                    if (listado.Count > 0)
                    {
                        Modulos = new ObservableCollection<ModuloDTO>(listado);
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
