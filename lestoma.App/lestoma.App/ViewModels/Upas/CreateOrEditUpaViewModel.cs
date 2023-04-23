using Acr.UserDialogs;
using lestoma.App.Models;
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
using Xamarin.Forms.Internals;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateOrEditUpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private UpaModel _model;
        private UpaRequest _upa;
        private bool _isVisibleProtocols;
        private bool _isCreate;
        private ObservableCollection<ProtocoloModel> _protocolos;
        public CreateOrEditUpaViewModel(INavigationService navigationService, IApiService apiService)
           : base(navigationService)
        {
            _model = new UpaModel();
            _model.AddValidationRules();
            _apiService = apiService;
            _upa = new UpaRequest();
            _isCreate = true;
            _protocolos = new ObservableCollection<ProtocoloModel>();
            CreateOrEditCommand = new Command(CreateOrEditClicked);
            DeleteCommand = new Command<object>(DeleteClicked, CanNavigate);
        }
        public UpaRequest Upa
        {
            get => _upa;
            set => SetProperty(ref _upa, value);
        }

        public UpaModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public bool IsVisibleProtocols
        {
            get => _isVisibleProtocols;
            set => SetProperty(ref _isVisibleProtocols, value);
        }

        public bool IsCreate
        {
            get => _isCreate;
            set => SetProperty(ref _isCreate, value);
        }

        public ObservableCollection<ProtocoloModel> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }
        public Command DeleteCommand { get; set; }
        public Command CreateOrEditCommand { get; }
        public Command AddProtocolCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await NavigationService.NavigateAsync(nameof(CreateEditProtocolPopupPage));
                });
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("upa") || Upa.Id != Guid.Empty)
            {
                Upa = parameters.GetValue<UpaRequest>("upa");
                Title = "Editar";
                IsVisibleProtocols = false;
                IsCreate = false;
            }
            else
            {
                if (parameters.ContainsKey("protocolo"))
                {
                    var protocolo = parameters.GetValue<ProtocoloModel>("protocolo");
                    Protocolos.Add(protocolo);
                    if (Protocolos.Count > 0)
                        IsVisibleProtocols = true;
                }
                Title = "Crear";
            }
        }
        private bool CanNavigate(object arg)
        {
            return true;
        }
        private void CargarDatos()
        {
            Model.Nombre.Value = Upa != null ? Upa.Nombre : string.Empty;
            Model.CantidadActividades.Value = Upa == null ? string.Empty : Upa.CantidadActividades
                > 0 ? Upa.CantidadActividades.ToString() : string.Empty;
            Model.Descripcion.Value = Upa != null ? Upa.Descripcion : string.Empty;
        }
        private void DeleteClicked(object obj)
        {
            ProtocoloModel protocol = (ProtocoloModel)obj;
            if (protocol == null)
                return;
            try
            {
                UserDialogs.Instance.ShowLoading("Eliminando...");
                var select = Protocolos.Remove(protocol);
                if (Protocolos.Count == 0)
                {
                    IsVisibleProtocols = false;
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
        private async void CreateOrEditClicked(object obj)
        {
            try
            {
                CargarDatos();

                if (_model.AreFieldsValid())
                {
                    if (IsCreate && Protocolos.Count == 0)
                    {
                        AlertWarning("Agregue un protocolo.");
                        return;
                    }
                    UserDialogs.Instance.ShowLoading("Guardando...");
                    if (!_apiService.CheckConnection())
                    {
                        AlertNoInternetConnection();
                        return;
                    }
                    UpaRequest request = new UpaRequest
                    {
                        Id = Upa.Id == Guid.Empty ? Guid.Empty : Upa.Id,
                        Nombre = _model.Nombre.Value.Trim(),
                        Descripcion = _model.Descripcion.Value.Trim(),
                        CantidadActividades = (short)Convert.ToInt32(_model.CantidadActividades.Value),
                        ProtocolosCOM = new List<ProtocoloRequest>()
                    };
                    Protocolos.ForEach(x => request.ProtocolosCOM.Add(new ProtocoloRequest
                    {
                        Nombre = x.Nombre,
                        PrimerByteTrama = (byte)x.PrimerByteTrama,
                        Sigla = x.Sigla
                    }));
                    if (Upa.Id == Guid.Empty)
                    {
                        ResponseDTO respuesta = await _apiService.PostAsyncWithToken(URL_API, "upas/crear", request, TokenUser.Token);
                        if (!respuesta.IsExito)
                        {
                            AlertWarning(respuesta.MensajeHttp);
                            return;
                        }
                        AlertSuccess(respuesta.MensajeHttp);
                        var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                        await NavigationService.GoBackAsync(parameters);
                    }
                    else
                    {
                        ResponseDTO respuesta = await _apiService.PutAsyncWithToken(URL_API, "upas/editar", request, TokenUser.Token);
                        if (!respuesta.IsExito)
                        {
                            AlertWarning(respuesta.MensajeHttp);
                            return;
                        }
                        AlertSuccess(respuesta.MensajeHttp);
                        var parameters = new NavigationParameters { { Constants.REFRESH, true } };
                        await NavigationService.GoBackAsync(parameters);
                    }
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
