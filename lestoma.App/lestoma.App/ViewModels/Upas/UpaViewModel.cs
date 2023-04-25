﻿using Acr.UserDialogs;
using lestoma.App.Views.Upas;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class UpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<UpaDTO> _upas;
        private Command<object> itemTap;
        private bool _isNavigating = false;
        public UpaViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;
            _upas = new ObservableCollection<UpaDTO>();
            EditCommand = new Command<object>(UpaSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
            SeeProtocolsCommand = new Command<object>(OnSeeProtocolClicked, CanNavigate);
            LoadUpas();
            MoreUpasCommand = new Command(LoadMoreUpas);
        }
        public ObservableCollection<UpaDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }

        public Command EditCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command MoreUpasCommand { get; set; }
        public Command<object> SeeProtocolsCommand
        {
            get => itemTap;
            set => SetProperty(ref itemTap, value);
        }
        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await NavigationService.NavigateAsync(nameof(CreateOrEditUpaPage));
                });
            }
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                this.Page = 1;
                LoadUpas();
            }
        }
        private async void UpaSelected(object objeto)
        {
            if (!_isNavigating)
            {
                _isNavigating = true;
                Syncfusion.ListView.XForms.ItemTappedEventArgs lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;

                if (!(lista.ItemData is UpaDTO upa))
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
                await NavigationService.NavigateAsync(nameof(CreateOrEditUpaPage), parameters);
                _isNavigating = false;
            }
        }

        private async void OnSeeProtocolClicked(object obj)
        {
            UpaDTO detalle = (UpaDTO)obj;
            if (detalle == null)
                return;
            var parameters = new NavigationParameters
            {
                { "protocolos", detalle.ProtocolosCOM }
            };
            await NavigationService.NavigateAsync($"{nameof(InfoProtocolPopupPage)}", parameters);
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }
        private void LoadUpas()
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
            UpaDTO detalle = (UpaDTO)obj;
            if (detalle == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API, "upas", detalle.Id, TokenUser.Token);
                    if (response.IsExito)
                    {
                        AlertSuccess(response.MensajeHttp);
                        this.Page = 1;
                        LoadUpas();
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
                Upas = new ObservableCollection<UpaDTO>();
                var paginacion = new Paginacion
                {
                    Page = this.Page,
                    PageSize = this.PageSize
                };
                string querystring = Reutilizables.GenerateQueryString(paginacion);
                ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<UpaDTO>(URL_API, $"upas/paginar{querystring}", TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertWarning(response.MensajeHttp);
                    return;
                }
                var paginador = (Paginador<UpaDTO>)response.Data;
                if (paginador.Datos.Any())
                {
                    Upas = new ObservableCollection<UpaDTO>(paginador.Datos);
                    IsRefreshing = paginador.HasNextPage;
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                IsBusy = false;
            }
        }
        private async void LoadMoreUpas()
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                    var paginacion = new Paginacion
                    {
                        Page = this.Page,
                        PageSize = this.PageSize
                    };
                    string querystring = Reutilizables.GenerateQueryString(paginacion);
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<UpaDTO>(URL_API, $"upas/paginar{querystring}", TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertWarning(response.MensajeHttp);
                        return;
                    }
                    var paginador = (Paginador<UpaDTO>)response.Data;
                    if (paginador.Datos.Any())
                    {
                        foreach (var item in paginador.Datos)
                        {
                            Upas.Add(item);
                        }
                        IsRefreshing = paginador.HasNextPage;
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                UserDialogs.Instance.HideLoading();
            }
        }

    }
}
