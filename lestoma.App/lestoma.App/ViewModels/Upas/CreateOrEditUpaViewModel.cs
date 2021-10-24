using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateOrEditUpaViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private UpaModel _model;
        private UpaRequest _upa;
        public Command CreateOrEditCommand { get; }

        public CreateOrEditUpaViewModel(INavigationService navigationService, IApiService apiService)
           : base(navigationService)
        {
            _model = new UpaModel();
            _model.AddValidationRules();
            _navigationService = navigationService;
            _apiService = apiService;
            _upa = new UpaRequest();
            CreateOrEditCommand = new Command(CreateOrEditarClicked);
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
        private void CargarDatos()
        {
            Model.Nombre.Value = Upa != null ? Upa.Nombre : string.Empty;
            Model.CantidadActividades.Value = Upa == null ? string.Empty : Upa.CantidadActividades > 0 ? Upa.CantidadActividades.ToString() : string.Empty;
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

        private async void CreateOrEditarClicked(object obj)
        {
            try
            {
                CargarDatos();
                if (_model.AreFieldsValid())
                {
                    UpaRequest request = new UpaRequest
                    {
                        Id = Upa.Id > 0 ? Upa.Id : 0,
                        Nombre = _model.Nombre.Value.Trim(),
                        Descripcion = _model.Descripcion.Value.Trim(),
                        CantidadActividades = (short)Convert.ToInt32(_model.CantidadActividades.Value)
                    };
                    if (!_apiService.CheckConnection())
                    {
                        CrossToastPopUp.Current.ShowToastError("Error no hay conexión a internet.");
                        return;
                    }
                    else
                    {
                        await _navigationService.NavigateAsync($"{nameof(LoadingPopupPage)}");
                      
                        if (Upa.Id == 0)
                        {
                            Response respuesta = await _apiService.PostAsyncWithToken(URL, "upas/crear", request, TokenUser.Token);
                            if (!respuesta.IsExito)
                            {
                                CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                                await _navigationService.ClearPopupStackAsync();
                                return;
                            }
                            CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                            await Task.Delay(2000);
                        }
                        else
                        {
                            Response respuesta = await _apiService.PutAsyncWithToken(URL, "upas/editar", request, TokenUser.Token);
                            if (!respuesta.IsExito)
                            {
                                CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                                await _navigationService.ClearPopupStackAsync();
                                return;
                            }
                            CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                            await Task.Delay(2000);
                        }
                        await _navigationService.ClearPopupStackAsync();
                        await _navigationService.GoBackAsync(null, useModalNavigation: true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                await _navigationService.ClearPopupStackAsync();
                Debug.WriteLine(ex.Message);
            } 
        }
    }
}
