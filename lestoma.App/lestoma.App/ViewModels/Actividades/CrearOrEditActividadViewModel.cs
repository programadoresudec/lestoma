using lestoma.App.Models;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.Logica;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        private void CargarDatos()
        {
            Model.Nombre.Value = Actividad != null ? Actividad.Nombre : string.Empty;
        }

        private async void CreateOrEditarClicked(object obj)
        {
            try
            {
                CargarDatos();
                if (_model.AreFieldsValid())
                {
                    ActividadRequest request = new ActividadRequest
                    {
                        Id = Actividad.Id > 0 ? Actividad.Id : 0,
                        Nombre = _model.Nombre.Value
                    };
                    if (!_apiService.CheckConnection())
                    {
                        LSActividad _actividadOfflineService = new LSActividad(App.DbPathSqlLite);
                        await _actividadOfflineService.CrearAsync(request);
                    }
                    else
                    {
                        string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
                        TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);

                        if (Actividad.Id == 0)
                        {
                            Response respuesta = await _apiService.PostAsyncWithToken(url, "Actividad/crear", request, UserApp.Token);
                            if (!respuesta.IsExito)
                            {
                                CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                                return;
                            }
                            CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                            await Task.Delay(2000);
                        }
                        else
                        {
                            Response respuesta = await _apiService.PutAsyncWithToken(url, "Actividad/editar", request, UserApp.Token);
                            if (!respuesta.IsExito)
                            {
                                CrossToastPopUp.Current.ShowToastError("Error " + respuesta.Mensaje);
                                return;
                            }
                            CrossToastPopUp.Current.ShowToastSuccess(respuesta.Mensaje);
                            await Task.Delay(2000);
                        }
                        await _navigationService.GoBackAsync(null, useModalNavigation: true, true);
                    }

                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        public Command CreateOrEditCommand { get; set; }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("actividad"))
            {
                Actividad = parameters.GetValue<ActividadRequest>("actividad");
                Title = "Editar";
            }
            else
            {
                Title = "Crear";
            }
        }
    }
}
