using lestoma.App.Models;
using lestoma.App.Validators.Rules;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Actividades
{

    public class CrearOrEditActividadPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ParametrosModel _model;

        public CrearOrEditActividadPageViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _model = new ParametrosModel();
            _model.AddValidationRules();
            CreateOrEditCommand = new Command(createOrEditarClicked);
        }

        private async void createOrEditarClicked(object obj)
        {
            try
            {
                if (_model.AreFieldsValid())
                {

                    if (!_apiService.CheckConnection())
                    {
                        return;
                    }
                    else
                    {
                        string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();

                        ActividadRequest request = new ActividadRequest
                        {
                            Nombre = _model.Nombre.Value
                        };
                        TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
                        Response respuesta = await _apiService.PostAsyncWithToken(url, "Actividad/crear", request, UserApp.Token);
                        if (!respuesta.IsExito)
                        {
                            CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                            return;
                        }
                        CrossToastPopUp.Current.ShowToastSuccess( respuesta.Mensaje);
                        await Task.Delay(2000);
                        await _navigationService.GoBackAsync(null, useModalNavigation: true, true);
                    }

                }
               
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {

            }

        }

        public ParametrosModel Model
        {
            get => _model;
            set
            {
                if (_model == value)
                {
                    return;
                }
                SetProperty(ref _model, value);
            }
        }

        public Command CreateOrEditCommand { get; set; }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("actividad"))
            {
                var parametros = parameters.GetValue<ActividadRequest>("actividad");
                Model.Nombre.Value = parametros.Nombre;
            }
        }
    }
}
