﻿using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Modulos
{
    public class CreateOrEditModuloViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ParametrosModel _model;
        private ModuloRequest _modulo;
        public CreateOrEditModuloViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
            _modulo = new ModuloRequest();
            _model = new ParametrosModel();
            _model.AddValidationRules();
            CreateOrEditCommand = new Command(CreateOrEditarClicked);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("modulo") || Modulo.Id != Guid.Empty)
            {
                Modulo = parameters.GetValue<ModuloRequest>("modulo");
                Title = "Editar";
            }
            else
            {
                Title = "Crear";
            }
        }

        public Command CreateOrEditCommand { get; set; }

        public ModuloRequest Modulo
        {
            get => _modulo;
            set
            {
                SetProperty(ref _modulo, value);
            }
        }

        public ParametrosModel Model
        {
            get => _model;
            set
            {
                SetProperty(ref _model, value);
            }
        }


        private void CargarDatos()
        {
            Model.Nombre.Value = Modulo != null ? Modulo.Nombre : string.Empty;
        }

        private async void CreateOrEditarClicked(object obj)
        {
            ResponseDTO respuesta;
            try
            {
                CargarDatos();
                if (_model.AreFieldsValid())
                {
                    UserDialogs.Instance.ShowLoading("Guardando...");
                    if (_apiService.CheckConnection())
                    {
                        ModuloRequest request = new ModuloRequest
                        {
                            Id = Modulo.Id != Guid.Empty ? Modulo.Id : Guid.Empty,
                            Nombre = _model.Nombre.Value
                        };
                        if (Modulo.Id == Guid.Empty)
                        {
                            respuesta = await _apiService.PostAsyncWithToken(URL_API, "modulos/crear", request, TokenUser.Token);
                        }
                        else
                        {
                            respuesta = await _apiService.PutAsyncWithToken(URL_API, "modulos/editar", request, TokenUser.Token);
                        }
                        if (respuesta.IsExito)
                        {
                            AlertSuccess(respuesta.MensajeHttp);
                            var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                            await NavigationService.GoBackAsync(parameters, useModalNavigation: true, true);
                        }
                        else
                        {
                            AlertError(respuesta.MensajeHttp);
                        }
                        ClosePopup();
                    }
                    else
                    {
                        AlertNoInternetConnection();
                    }
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
