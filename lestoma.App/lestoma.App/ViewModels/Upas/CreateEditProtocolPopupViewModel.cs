using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateEditProtocolPopupViewModel : BaseViewModel
    {
        private ProtocoloModel _protocolo;
        private bool _isEdit;
        private int _idProtocolo;
        private readonly IApiService _apiService;
        private ObservableCollection<ProtocoloModel> _protocolos;
        private Guid _upaId;
        public CreateEditProtocolPopupViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _protocolo = new ProtocoloModel();
            SaveCommand = new Command(SaveClicked);
            Bytes = LoadBytes();
            _apiService = apiService;
            LoadProtocols();
        }

        private void LoadProtocols()
        {
            _protocolos = new ObservableCollection<ProtocoloModel>()
            {
                new ProtocoloModel()
                {
                    Id = 1,
                    Nombre ="Peer To Peer",
                    PrimerByteTrama = 73,
                    Sigla ="P2P"
                },
                new ProtocoloModel()
                {
                    Id = 2,
                    Nombre ="Broadcast",
                    PrimerByteTrama = 111,
                    Sigla ="BR"
                }
            };
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
                    var comunicacion = Protocolos.Where(x => x.Nombre == protocolo.Nombre).FirstOrDefault();
                    Protocolo = comunicacion;
                    _idProtocolo = protocolo.Id;
                }
            }
            else if (parameters.ContainsKey("upaId"))
            {
                _upaId = parameters.GetValue<Guid>("upaId");
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
        public ObservableCollection<ProtocoloModel> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }
        private async void SaveClicked(object obj)
        {
            try
            {
                if (_upaId != Guid.Empty)
                {
                    UserDialogs.Instance.ShowLoading("Guardando...");
                    if (!_apiService.CheckConnection())
                    {
                        AlertNoInternetConnection();
                        return;
                    }
                    ProtocoloRequest protocoloRequest = new ProtocoloRequest
                    {
                        Nombre = _protocolo.Nombre,
                        PrimerByteTrama = (byte)_protocolo.PrimerByteTrama,
                        Sigla = _protocolo.Sigla,
                        UpaId = _upaId
                    };
                    ResponseDTO respuesta = await _apiService.PostAsyncWithToken(URL_API, "upas/agregar-protocolo", protocoloRequest, TokenUser.Token);
                    if (!respuesta.IsExito)
                    {
                        AlertWarning(respuesta.MensajeHttp);
                        return;
                    }
                    AlertSuccess(respuesta.MensajeHttp);
                    var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                    await NavigationService.GoBackAsync(parameters);
                }
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
                        Id = _idProtocolo,
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
                    await NavigationService.ClearPopupStackAsync(parameters);
                }
                else
                {
                    var parameters = new NavigationParameters { { "protocolo", _protocolo } };
                    await NavigationService.GoBackAsync(parameters);
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
