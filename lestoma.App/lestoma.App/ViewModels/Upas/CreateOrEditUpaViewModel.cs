using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateOrEditUpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private UpaModel _model;
        private UpaRequest _upa;
        public CreateOrEditUpaViewModel(INavigationService navigationService, IApiService apiService)
           : base(navigationService)
        {
            _model = new UpaModel();
            _model.AddValidationRules();
            _apiService = apiService;
            _upa = new UpaRequest();
            CreateOrEditCommand = new Command(CreateOrEditClicked);
        }
        public UpaRequest Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
            }
        }

        public UpaModel Model
        {
            get => _model;
            set
            {
                SetProperty(ref _model, value);
            }
        }
        public Command CreateOrEditCommand { get; }

        private void CargarDatos()
        {
            Model.Nombre.Value = Upa != null ? Upa.Nombre : string.Empty;
            Model.CantidadActividades.Value = Upa == null ? string.Empty : Upa.CantidadActividades
                > 0 ? Upa.CantidadActividades.ToString() : string.Empty;
            Model.Descripcion.Value = Upa != null ? Upa.Descripcion : string.Empty;
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("upa"))
            {
                Upa = parameters.GetValue<UpaRequest>("upa");
                Title = "Editar";
            }
            else
            {
                Title = "Crear";
            }
        }

        private async void CreateOrEditClicked(object obj)
        {
            try
            {
                CargarDatos();
                if (_model.AreFieldsValid())
                {
                    UpaRequest request = new UpaRequest
                    {
                        Id = Upa.Id == Guid.Empty ? Guid.Empty : Upa.Id,
                        Nombre = _model.Nombre.Value.Trim(),
                        Descripcion = _model.Descripcion.Value.Trim(),
                        CantidadActividades = (short)Convert.ToInt32(_model.CantidadActividades.Value)
                    };
                    if (_apiService.CheckConnection())
                    {
                        await _navigationService.NavigateAsync($"{nameof(LoadingPopupPage)}");

                        if (Upa.Id == Guid.Empty)
                        {
                            ResponseDTO respuesta = await _apiService.PostAsyncWithToken(URL_API, "upas/crear", request, TokenUser.Token);
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
                            ResponseDTO respuesta = await _apiService.PutAsyncWithToken(URL_API, "upas/editar", request, TokenUser.Token);
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
                if (PopupNavigation.Instance.PopupStack.Any())
                    await PopupNavigation.Instance.PopAsync();
            }
        }
    }
}
