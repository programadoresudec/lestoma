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
        private ObservableCollection<ActividadRequest> _actividades;
        public ActividadViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            EditCommand = new Command<object>(ActividadSelected, CanNavigate);
            LoadActividades();
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
                        "Actividad", ItemDelete.Id, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                        return;
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
        private async void ActividadSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var actividad = lista.ItemData as ActividadRequest;

            if (actividad == null)
                return;
            var parameters = new NavigationParameters
            {
                { "actividad", actividad }
            };
            await _navigationService.NavigateAsync(nameof(CrearOrEditActividadPage), parameters, useModalNavigation: true, true);

        }

        public ObservableCollection<ActividadRequest> Actividades
        {
            get => _actividades;
            set => SetProperty(ref _actividades, value);
        }
        public ActividadRequest ItemDelete { get; set; }


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
                InsertarListadoActividades();
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
            LSActividad _actividadOfflineService = new LSActividad(App.DbPathSqlLite);
            if (!_apiService.CheckConnection())
            {
                var query = await _actividadOfflineService.GetAll();
                if (query.Count > 0)
                {
                    Actividades = new ObservableCollection<ActividadRequest>(query);
                }
            }
            else
            {
                Response response = await _apiService.GetListAsyncWithToken<List<ActividadRequest>>(URL,
              "Actividad/listado", TokenUser.Token);
                var query = await _actividadOfflineService.GetAll();
                await _actividadOfflineService.MergeEntity((List<ActividadRequest>)response.Data);
                var query2 = await _actividadOfflineService.GetAll();

                if (!response.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                    return;
                }
                if (response.Data == null)
                {
                    Actividades = new ObservableCollection<ActividadRequest>();
                    return;
                }
                Actividades = new ObservableCollection<ActividadRequest>((List<ActividadRequest>)response.Data);
            }

        }
    }
}
