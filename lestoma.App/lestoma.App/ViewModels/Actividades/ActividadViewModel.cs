using lestoma.App.Views;
using lestoma.App.Views.Actividades;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
            EditCommand = new Command<object>(ActividadSelected, CanNavigate);
            ServiceListadoActividades();
        }
        public Command EditCommand { get; set; }

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
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("refresh"))
            {
                ServiceListadoActividades(true);
            }
        }
        public async void DeleteClicked()
        {
            if (ItemDelete != null)
            {
                try
                {
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                    if (_apiService.CheckConnection())
                    {
                        ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API,
                        "actividades", ItemDelete.Id, TokenUser.Token);
                        if (response.IsExito)
                        {
                            AlertSuccess(response.MensajeHttp);
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
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    await _navigationService.ClearPopupStackAsync();
                }
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
        public ActividadDTO ItemDelete { get; set; }

        public void LoadActividades()
        {
            if (_apiService.CheckConnection())
            {
                ServiceListadoActividades();
            }
            else
            {
                AlertNoInternetConnection();
            }
        }

        private async void ServiceListadoActividades(bool refresh = false)
        {
            try
            {
                if (!refresh)
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));

                Actividades = new ObservableCollection<ActividadDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<ActividadDTO>>(URL_API, "actividades/listado", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<ActividadDTO>)response.Data;
                    Actividades = new ObservableCollection<ActividadDTO>(listado);
                    ClosePopup();
                }
            }
            catch (Exception ex)
            {
                if (!refresh)
                    if (PopupNavigation.Instance.PopupStack.Any())
                        await PopupNavigation.Instance.PopAsync();
                SeeError(ex);
            }

        }
    }
}
