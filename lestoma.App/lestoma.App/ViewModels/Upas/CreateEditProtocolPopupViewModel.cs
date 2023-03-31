using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateEditProtocolPopupViewModel : BaseViewModel
    {
        private ProtocoloModel _protocolo;
        private bool _isEdit;
        private readonly IApiService _apiService;

        public CreateEditProtocolPopupViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _protocolo = new ProtocoloModel();
            SaveCommand = new Command(SaveClicked);
            Bytes = LoadBytes();
            _apiService = apiService;
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("dataProtocolo"))
            {
                var protocolo = parameters.GetValue<ProtocoloRequest>("dataProtocolo");
                if (protocolo != null)
                {
                    IsEdit = true;
                    var byteResult = Bytes.Where(x => x == protocolo.PrimerByteTrama).FirstOrDefault();
                    Protocolo = new ProtocoloModel
                    {
                        Id = protocolo.Id,
                        Nombre = protocolo.Nombre,
                        PrimerByteTrama = byteResult,
                        Sigla = protocolo.Sigla
                    };
                }

            }
        }
        private List<int> LoadBytes()
        {
            return Enumerable.Range(0, 256).ToList();
        }

        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        public Command SaveCommand { get; set; }
        public List<int> Bytes { get; set; }

        public ProtocoloModel Protocolo
        {
            get => _protocolo;
            set => SetProperty(ref _protocolo, value);
        }
        private async void SaveClicked(object obj)
        {
            try
            {
                if (IsEdit)
                {
                    UserDialogs.Instance.ShowLoading("Guardando...");
                    if (!_apiService.CheckConnection())
                    {
                        AlertNoInternetConnection();
                        return;
                    }
                    ProtocoloRequest protocoloRequest = new ProtocoloRequest
                    {
                        Id = _protocolo.Id,
                        Nombre = _protocolo.Nombre,
                        PrimerByteTrama = (byte)_protocolo.PrimerByteTrama,
                        Sigla = _protocolo.Sigla
                    };

                    ResponseDTO respuesta = await _apiService.PutAsyncWithToken(URL_API, "upas/editar-protocolo", protocoloRequest, TokenUser.Token);
                    if (!respuesta.IsExito)
                    {
                        AlertWarning(respuesta.MensajeHttp);
                        return;
                    }
                    AlertSuccess(respuesta.MensajeHttp);
                    var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                    await _navigationService.ClearPopupStackAsync(parameters);
                }
                else
                {
                    var parameters = new NavigationParameters { { "protocolo", _protocolo } };
                    await _navigationService.GoBackAsync(parameters);
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
    }
}
