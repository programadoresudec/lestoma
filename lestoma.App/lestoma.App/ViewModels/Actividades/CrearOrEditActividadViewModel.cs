using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Actividades
{

    public class CrearOrEditActividadViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ParametrosModel _model;
        private ActividadRequest _actividad;
        public CrearOrEditActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _actividad = new ActividadRequest();
            _model = new ParametrosModel();
            _model.AddValidationRules();
            CreateOrEditCommand = new Command(CreateOrEditarClicked);
        }
        public Command CreateOrEditCommand { get; set; }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("actividad") || Actividad.Id != Guid.Empty)
            {
                Actividad = parameters.GetValue<ActividadRequest>("actividad");
                Title = "Editar";
            }
            else
            {
                Title = "Crear";
            }
        }
        public ActividadRequest Actividad
        {
            get => _actividad;
            set
            {
                SetProperty(ref _actividad, value);
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
            Model.Nombre.Value = Actividad != null ? Actividad.Nombre : string.Empty;
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
                        ActividadRequest request = new ActividadRequest
                        {
                            Id = Actividad.Id != Guid.Empty ? Actividad.Id : Guid.Empty,
                            Nombre = _model.Nombre.Value
                        };
                        if (Actividad.Id == Guid.Empty)
                        {
                            respuesta = await _apiService.PostAsyncWithToken(URL_API, "actividades/crear", request, TokenUser.Token);
                        }
                        else
                        {
                            respuesta = await _apiService.PutAsyncWithToken(URL_API, "actividades/editar", request, TokenUser.Token);
                        }
                        if (respuesta.IsExito)
                        {
                            AlertSuccess(respuesta.MensajeHttp);
                            var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                            await _navigationService.GoBackAsync(parameters, useModalNavigation: true, true);
                        }
                        else
                        {
                            AlertWarning(respuesta.MensajeHttp);
                        }
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
