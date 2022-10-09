using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
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
        public Command CreateOrEditCommand { get; set; }
        private void CargarDatos()
        {
            Model.Nombre.Value = Modulo != null ? Modulo.Nombre : string.Empty;
        }

        private async void CreateOrEditarClicked(object obj)
        {
            Response respuesta;
            try
            {
                CargarDatos();
                if (_model.AreFieldsValid())
                {
                    ModuloRequest request = new ModuloRequest
                    {
                        Id = Modulo.Id != Guid.Empty ? Modulo.Id : Guid.Empty,
                        Nombre = _model.Nombre.Value
                    };
                    if (_apiService.CheckConnection())
                    {
                        await PopupNavigation.Instance.PushAsync(new LoadingPopupPage("Creando..."));
                        if (Modulo.Id == Guid.Empty)
                        {
                            respuesta = await _apiService.PostAsyncWithToken(URL, "modulos/crear", request, TokenUser.Token);
                        }
                        else
                        {
                            respuesta = await _apiService.PutAsyncWithToken(URL, "modulos/editar", request, TokenUser.Token);
                        }
                        if (respuesta.IsExito)
                        {
                            AlertSuccess(respuesta.Mensaje);
                            await _navigationService.GoBackAsync(null, useModalNavigation: true, true);
                        }
                        else
                        {
                            AlertError(respuesta.Mensaje);
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
                await PopupNavigation.Instance.PopAsync();
                SeeError(ex);
            }
        }

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


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("modulo"))
            {
                Modulo = parameters.GetValue<ModuloRequest>("modulo");
                Title = "Editar";
            }
            else if (Modulo != null)
            {
                Title = "Editar";
            }
            else
            {
                Title = "Crear";
            }
        }
    }
}
