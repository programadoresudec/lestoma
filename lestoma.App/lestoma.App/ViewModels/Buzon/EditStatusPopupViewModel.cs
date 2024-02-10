using Acr.UserDialogs;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.ListadosJson;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Buzon
{
    public class EditStatusPopupViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private EstadoBuzonDTO _estado;
        private ObservableCollection<EstadoBuzonDTO> _estados;
        private int _buzonId;
        public EditStatusPopupViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
            LoadEstados();
            SaveCommand = new Command(SaveClicked);
        }


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("BuzonId"))
            {
                _buzonId = parameters.GetValue<int>("BuzonId");
            }
        }
        public EstadoBuzonDTO EstadoBuzon
        {
            get => _estado;
            set => SetProperty(ref _estado, value);
        }
        public ObservableCollection<EstadoBuzonDTO> EstadosBuzon
        {
            get => _estados;
            set => SetProperty(ref _estados, value);
        }
        public Command SaveCommand { get; set; }

        private void LoadEstados()
        {
            var data = new ListadoEstadoBuzon().Listado;
            EstadosBuzon = new ObservableCollection<EstadoBuzonDTO>(data);
        }
        private async void SaveClicked(object obj)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Guardando...");
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                EditarEstadoBuzonRequest Request = new EditarEstadoBuzonRequest
                {
                  EstadoBuzon = this.EstadoBuzon,
                  BuzonId = _buzonId,
                };

                ResponseDTO respuesta = await _apiService.PutAsyncWithToken(URL_API, "buzon-de-reportes/edit-status", Request, TokenUser.Token);
                if (!respuesta.IsExito)
                {
                    AlertWarning(respuesta.MensajeHttp);
                    return;
                }
                AlertSuccess(respuesta.MensajeHttp);
                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                await NavigationService.ClearPopupStackAsync(parameters);
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
    }
}
