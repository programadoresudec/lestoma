﻿using Acr.UserDialogs;
using lestoma.App.Views.Upas;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class UpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<UpaDTO> _upas;
        public UpaViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;
            _upas = new ObservableCollection<UpaDTO>();
            EditCommand = new Command<object>(UpaSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
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

        public Command EditCommand { get; set; }
        public Command DeleteCommand { get; set; }
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
            if (parameters.ContainsKey("refresh"))
            {
                LoadUpas();
            }
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
                        ConsumoService();
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
                UserDialogs.Instance.ShowLoading("Cargando...");
                Upas = new ObservableCollection<UpaDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<UpaDTO>>(URL_API,
                    $"upas/listado", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<UpaDTO>)response.Data;
                    Upas = new ObservableCollection<UpaDTO>(listado);
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
                UserDialogs.Instance.HideLoading();
            }
        }
    }
}
