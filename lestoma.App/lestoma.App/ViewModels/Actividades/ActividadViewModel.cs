using lestoma.App.Views;
using lestoma.App.Views.Actividades;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using lestoma.DatabaseOffline.Logica;
using Plugin.Toast;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Actividades
{
    public class ActividadViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<ActividadDTO> _actividades;
        private LSActividad _actividadOfflineService;
        public ActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _actividadOfflineService = new LSActividad(App.DbPathSqlLite);
            EditCommand = new Command<object>(ActividadSelected, CanNavigate);
            LoadActividades();
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
            RefreshActividades();
        }
        public async void DeleteClicked()
        {
            if (ItemDelete != null)
            {
                try
                {
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                    if (!_apiService.CheckConnection())
                    {
                        CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                        return;
                    }
                    Response response = await _apiService.DeleteAsyncWithToken(URL,
                        "actividades", ItemDelete.Id, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                        return;
                    }
                    Response response1 = await _apiService.GetListAsyncWithToken<List<ActividadDTO>>(URL, "actividades/listado", TokenUser.Token);
                    if (response.IsExito)
                    {
                        _actividadOfflineService.MergeEntity((List<ActividadDTO>)response1.Data);
                    }
                    CrossToastPopUp.Current.ShowToastSuccess(response.Mensaje);
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


        public void RefreshActividades()
        {
            try
            {
                InsertarListadoActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public async void LoadActividades()
        {
            try
            {
                await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                if (_apiService.CheckConnection())
                {
                    InsertarListadoActividades();
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

        private async void InsertarListadoActividades()
        {
            Actividades = new ObservableCollection<ActividadDTO>();
            Response response = await _apiService.GetListAsyncWithToken<List<ActividadDTO>>(URL, "actividades/listado", TokenUser.Token);
            if (response.IsExito)
            {
                var listado = (List<ActividadDTO>)response.Data;
                Actividades = new ObservableCollection<ActividadDTO>(listado);
            }
          
         
        }
    }
}
