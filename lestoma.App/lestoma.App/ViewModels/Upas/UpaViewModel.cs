﻿using lestoma.App.Views;
using lestoma.App.Views.Upas;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class UpaViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<UpaDTO> _upas;


        public UpaViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _upas = new ObservableCollection<UpaDTO>();
            EditCommand = new Command<object>(UpaSelected, CanNavigate);
            LoadMoreItemsCommand = new Command<object>(LoadMoreItems, CanLoadMoreItems);
            LoadUpas();
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }

        public ObservableCollection<UpaDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }
        public UpaDTO ItemDelete { get; set; }

        public Command EditCommand { get; set; }
        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(CreateOrEditUpaPage), null, useModalNavigation: true, true);
                });
            }
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            ConsumoService();
        }
        private async void UpaSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var upa = lista.ItemData as UpaDTO;

            if (upa == null)
                return;

            UpaRequest upaEdit = new UpaRequest
            {
                CantidadActividades = upa.CantidadActividades,
                Descripcion = upa.Descripcion,
                Id = upa.Id,
                Nombre = upa.Nombre
            };
            var parameters = new NavigationParameters
            {
                { "upa", upaEdit }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditUpaPage), parameters, useModalNavigation: true, true);

        }
        private async void LoadMoreItems(object obj)
        {

            try
            {
                IsBusy = true;
                await Task.Delay(1000);
                AddUpas();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void AddUpas()
        {
            Page = ++Page;
            Response response = await _apiService.GetPaginadoAsyncWithToken<UpaDTO>(URL,
                $"upas/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }
            Paginador<UpaDTO> paginador = (Paginador<UpaDTO>)response.Data;
            TotalItems = paginador.TotalDatos;
            if (paginador.TotalPages <= 1)
            {
                return;
            }
            if (paginador.HasNextPage)
            {
                foreach (var item in paginador.Datos)
                {
                    Upas.Add(item);
                }
            }
            if (Upas.Count < TotalItems)
            {
                if (paginador.HasPreviousPage == true && paginador.HasNextPage == false)
                {
                    foreach (var item in paginador.Datos)
                    {
                        Upas.Add(item);
                    }
                }
            }
        }
        private bool CanLoadMoreItems(object obj)
        {

            if (Upas.Count >= TotalItems)
                return false;
            return true;
        }

        public Command<object> LoadMoreItemsCommand { get; set; }

        private async void LoadUpas()
        {
            await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
            try
            {
                if (!_apiService.CheckConnection())
                {
                    CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                    return;
                }
                ConsumoService();
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
                    if (!_apiService.CheckConnection())
                    {
                        CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                        return;
                    }
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                    Response response = await _apiService.DeleteAsyncWithToken(URL,
                        "upas", ItemDelete.Id, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                        return;
                    }
                    CrossToastPopUp.Current.ShowToastSuccess(response.Mensaje);
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
        private async void ConsumoService()
        {
            Upas = new ObservableCollection<UpaDTO>();
            Page = 1;
            Response response = await _apiService.GetPaginadoAsyncWithToken<UpaDTO>(URL,
                $"upas/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
            if (!response.IsExito)
            {
                CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                return;
            }
            Paginador<UpaDTO> paginador = (Paginador<UpaDTO>)response.Data;
            TotalItems = paginador.TotalDatos;
            if (paginador.HasNextPage)
            {
                if (Upas.Count > 0)
                {
                    foreach (var item in paginador.Datos)
                    {
                        Upas.Add(item);
                    }
                }
                else
                {
                    Upas = new ObservableCollection<UpaDTO>(paginador.Datos);
                }
            }
            else if (paginador.HasNextPage == false && paginador.HasPreviousPage == false)
            {
                foreach (var item in paginador.Datos)
                {
                    Upas.Add(item);
                }
            }
        }
    }
}