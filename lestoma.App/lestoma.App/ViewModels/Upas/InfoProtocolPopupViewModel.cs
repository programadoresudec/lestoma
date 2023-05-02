using Acr.UserDialogs;
using lestoma.App.Views.Upas;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class InfoProtocolPopupViewModel : BaseViewModel
    {
        private ObservableCollection<ProtocoloRequest> _protocolos;
        private readonly IApiService _apiService;
        public InfoProtocolPopupViewModel(INavigationService navigationService, IApiService apiService) : base(navigationService)
        {
            _apiService = apiService;
            EditProtocolCommand = new Command<object>(EditProtocolClicked, CanNavigate);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
        }
        public Command EditProtocolCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public ObservableCollection<ProtocoloRequest> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }
        private bool CanNavigate(object arg)
        {
            return true;
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("protocolos"))
            {
                var protocolos = parameters.GetValue<object>("protocolos");
                Protocolos = new ObservableCollection<ProtocoloRequest>((List<ProtocoloRequest>)protocolos);
            }
        }
        private async void EditProtocolClicked(object obj)
        {
            try
            {
                var list = obj as Syncfusion.ListView.XForms.ItemTappedEventArgs;

                if (!(list.ItemData is ProtocoloRequest protocolo))
                    return;
                var parameters = new NavigationParameters { { "dataProtocolo", protocolo } };
                await NavigationService.NavigateAsync($"{nameof(CreateEditProtocolPopupPage)}", parameters);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }
        private async void DeleteClicked(object obj)
        {
            ProtocoloRequest protocol = (ProtocoloRequest)obj;
            if (protocol == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                }
                ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API, "upas/eliminar-protocolo", protocol.Id, TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertWarning(response.MensajeHttp);
                    return;
                }
                AlertSuccess(response.MensajeHttp);
                var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                await NavigationService.GoBackAsync(parameters);
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
