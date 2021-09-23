using lestoma.App.Views;
using lestoma.App.Views.Actividades;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using Plugin.Toast;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using Syncfusion.SfBusyIndicator.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Actividades
{
    public class ActividadPageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<ActividadRequest> _actividades;
        public ActividadPageViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            EditCommand = new Command<object>(ActividadSelected, CanNavigate);
            DeleteCommand = new Command(DeleteClicked);
            loadActividades();
        }

        private void DeleteClicked()
        {
            if (ItemDelete != null)
            {

            }
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }
        public Command DeleteCommand { get; set; }
        public Command EditCommand { get; set; }

        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await Prism.PrismApplicationBase.Current.MainPage.Navigation.PushModalAsync(new CrearOrEditActividadPage());

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
            await Prism.PrismApplicationBase.Current.MainPage.Navigation.PushModalAsync(new CrearOrEditActividadPage());

        }

        public ObservableCollection<ActividadRequest> Actividades
        {
            get => _actividades;
            set => SetProperty(ref _actividades, value);
        }
        public ActividadRequest ItemDelete { get; set; }

        private async void loadActividades()
        {
            try
            {
                await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                if (!_apiService.CheckConnection())
                {
                    CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                    return;
                }
                string url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
                TokenDTO UserApp = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
                Response response = await _apiService.GetListAsyncWithToken<List<ActividadRequest>>(url,
                    "Actividad/listado", UserApp.Token);
                if (!response.IsExito)
                {
                    CrossToastPopUp.Current.ShowToastError("Error " + response.Mensaje);
                    return;
                }
                Actividades = new ObservableCollection<ActividadRequest>((List<ActividadRequest>)response.Data);
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
}
