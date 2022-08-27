using lestoma.App.Views;
using lestoma.App.Views.Modulos;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            ConsumoService();
        }

        private async void ConsumoService()
        {
            Modulos = new ObservableCollection<ModuloDTO>();
            Response response = await _apiService.GetListAsyncWithToken<List<ModuloDTO>>(URL,
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
        private async void LoadModulos()
        {
            await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                await _navigationService.ClearPopupStackAsync();
            }
        }

        public async void DeleteClicked()
        {
            if (ItemDelete != null)
            {
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                        Response response = await _apiService.DeleteAsyncWithToken(URL,
                            "modulos", ItemDelete.Id, TokenUser.Token);
                        if (!response.IsExito)
                        {

                            CrossToastPopUp.Current.ShowToastError($"ERROR: {response.Mensaje}", Plugin.Toast.Abstractions.ToastLength.Long);
                            return;
                        }
                        CrossToastPopUp.Current.ShowToastSuccess($"{response.Mensaje}", Plugin.Toast.Abstractions.ToastLength.Long);

                    }
                    else
                    {
                        CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.",
                    Plugin.Toast.Abstractions.ToastLength.Long);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    await _navigationService.ClearPopupStackAsync();
                }
            }
        }
    }
}
