﻿using lestoma.App.Views.UpasActividades;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class DetalleUpaActividadViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<DetalleUpaActividadDTO> _detalleUpaActividad;
        private Command<object> itemTap;
        private bool _isNavigating = false;
        public DetalleUpaActividadViewModel(INavigationService navigationService, IApiService apiService) :
        base(navigationService)
        {
            _apiService = apiService;
            _detalleUpaActividad = new ObservableCollection<DetalleUpaActividadDTO>();
            LoadDetalle();
            Title = "Asignar upa y actividades";
            LoadMoreItemsCommand = new Command<object>(LoadMoreItems, CanLoadMoreItems);
            EditCommand = new Command<object>(DetalleSelected, CanNavigate);
            SeeActivitiesCommand = new Command<object>(OnSeeActivityClicked, CanNavigate);
        }
        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await NavigationService.NavigateAsync(nameof(CreateOrEditDetalleUpaActividadPage));
                });
            }
        }
        public Command EditCommand { get; set; }
        public Command<object> LoadMoreItemsCommand { get; }

        public ObservableCollection<DetalleUpaActividadDTO> DetalleUpasActividades
        {
            get => _detalleUpaActividad;
            set => SetProperty(ref _detalleUpaActividad, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                DetalleUpasActividades.Clear();
                DetalleUpasActividades = new ObservableCollection<DetalleUpaActividadDTO>();
                ConsumoService();
            }
        }
        private bool CanNavigate(object arg)
        {
            return true;
        }
        private async void DetalleSelected(object objeto)
        {
            if (!_isNavigating)
            {
                _isNavigating = true;
                Syncfusion.ListView.XForms.ItemTappedEventArgs lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;

                if (!(lista.ItemData is DetalleUpaActividadDTO detalleUpaActividad))
                    return;
                var parameters = new NavigationParameters
                    {
                        { "detalleUpaActividad", detalleUpaActividad }
                    };
                await NavigationService.NavigateAsync(nameof(CreateOrEditDetalleUpaActividadPage), parameters);
                _isNavigating = false;
            }

        }
        private void LoadDetalle()
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
        public Command<object> SeeActivitiesCommand
        {
            get => itemTap;
            set => SetProperty(ref itemTap, value);
        }

        private async void OnSeeActivityClicked(object obj)
        {
            DetalleUpaActividadDTO detalle = (DetalleUpaActividadDTO)obj;
            if (detalle == null)
                return;
            var parameters = new NavigationParameters
            {
                { "filtroUserUpa", new UpaUserFilterRequest{UpaId = detalle.UpaId, UsuarioId = detalle.UsuarioId } }
            };
            await NavigationService.NavigateAsync($"{nameof(ActividadesByUsuarioPopupPage)}", parameters);
        }

        private async void ConsumoService()
        {
            try
            {
                IsBusy = true;
                ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<DetalleUpaActividadDTO>(URL_API,
               $"detalle-upas-actividades/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
                if (response.IsExito)
                {
                    Paginador<DetalleUpaActividadDTO> paginador = (Paginador<DetalleUpaActividadDTO>)response.Data;
                    TotalItems = paginador.TotalDatos;
                    if (paginador.HasNextPage)
                    {
                        Page += 1;
                        if (DetalleUpasActividades.Count > 0)
                        {
                            foreach (var item in paginador.Datos)
                            {
                                DetalleUpasActividades.Add(item);
                            }
                        }
                        else
                        {
                            DetalleUpasActividades = new ObservableCollection<DetalleUpaActividadDTO>(paginador.Datos);
                        }
                    }
                    else if (paginador.HasNextPage == false && paginador.HasPreviousPage == false)
                    {
                        foreach (var item in paginador.Datos)
                        {
                            DetalleUpasActividades.Add(item);
                        }
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
        private bool CanLoadMoreItems(object arg)
        {
            if (DetalleUpasActividades.Count >= TotalItems)
                return false;
            return true;
        }

        private void LoadMoreItems(object obj)
        {
            if (_apiService.CheckConnection())
            {
                AddUpasConActividades();
            }
            else
            {
                AlertNoInternetConnection();
            }
        }

        private async void AddUpasConActividades()
        {
            try
            {
                IsBusy = true;
                ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<DetalleUpaActividadDTO>(URL_API,
                    $"detalle-upas-actividades/paginar?Page={Page}&&PageSize={PageSize}", TokenUser.Token);
                if (response.IsExito)
                {
                    Paginador<DetalleUpaActividadDTO> paginador = (Paginador<DetalleUpaActividadDTO>)response.Data;
                    if (paginador.TotalPages <= 1)
                    {
                        return;
                    }
                    if (paginador.HasNextPage)
                    {

                        Page += 1;
                        foreach (var item in paginador.Datos)
                        {
                            DetalleUpasActividades.Add(item);
                        }

                    }
                    if (DetalleUpasActividades.Count < TotalItems)
                    {
                        if (paginador.HasPreviousPage == true && paginador.HasNextPage == false)
                        {
                            foreach (var item in paginador.Datos)
                            {
                                DetalleUpasActividades.Add(item);
                            }
                        }
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
