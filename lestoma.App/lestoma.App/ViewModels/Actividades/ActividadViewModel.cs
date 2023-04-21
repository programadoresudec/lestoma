using Acr.UserDialogs;
using lestoma.App.Views.Actividades;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Actividades
{
    public class ActividadViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<ActividadDTO> _actividades;
        public ActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _actividades = new ObservableCollection<ActividadDTO>();
            EditCommand = new Command<object>(ActividadSelected, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
            LoadActividades();
        }
        public Command EditCommand { get; set; }

        public Command DeleteCommand { get; set; }


        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(CrearOrEditActividadPage), null, useModalNavigation: true, true);

                });
            }
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                var Result = await ListadoActual();
                Actividades.Clear();
                Actividades = new ObservableCollection<ActividadDTO>(Result);
            }
        }
        public async void DeleteClicked(object obj)
        {
            ActividadDTO detalle = (ActividadDTO)obj;
            if (detalle == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                if (_apiService.CheckConnection())
                {
                    ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API,
                    "actividades", detalle.Id, TokenUser.Token);
                    if (response.IsExito)
                    {
                        AlertSuccess(response.MensajeHttp);
                        var Result = await ListadoActual();
                        Actividades.Clear();
                        Actividades = new ObservableCollection<ActividadDTO>(Result);
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

        private bool CanNavigate(object arg)
        {
            return true;
        }

        private async void ActividadSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var actividad = lista.ItemData as ActividadDTO;

            if (actividad == null)
                return;

            ActividadRequest request = new ActividadRequest
            {
                Id = actividad.Id,
                Nombre = actividad.Nombre
            };
            var parameters = new NavigationParameters
            {
                { "actividad", request }
            };
            await _navigationService.NavigateAsync(nameof(CrearOrEditActividadPage), parameters, useModalNavigation: true, true);

        }

        public ObservableCollection<ActividadDTO> Actividades
        {
            get => _actividades;
            set => SetProperty(ref _actividades, value);
        }

        public void LoadActividades()
        {
            if (!_apiService.CheckConnection())
            {
                AlertNoInternetConnection();
                return;

            }
            ServiceListadoActividades();
        }


        private async Task<List<ActividadDTO>> ListadoActual()
        {
            ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ActividadDTO>>(URL_API, "actividades/listado", TokenUser.Token);
            if (response.IsExito)
            {
                var listado = (List<ActividadDTO>)response.Data;
                return listado;
            }
            return null;
        }

        public async void ServiceListadoActividades()
        {
            try
            {
                IsBusy = true;
                Actividades.Clear();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ActividadDTO>>(URL_API, "actividades/listado", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<ActividadDTO>)response.Data;
                    Actividades = new ObservableCollection<ActividadDTO>(listado);
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
