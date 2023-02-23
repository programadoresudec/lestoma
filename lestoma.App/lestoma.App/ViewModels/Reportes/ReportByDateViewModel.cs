using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.App.Views.Reportes;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes
{
    public class ReportByDateViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private NameDTO _tipoArchivo;
        private ObservableCollection<NameDTO> _tipoArchivos;
        private bool _isSuperAdmin;
        private FiltroFechaModel _filtroFecha;

        public ReportByDateViewModel(INavigationService navigation, IApiService apiService)
            : base(navigation)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador ? true : false;
            _apiService = apiService;
            Title = "Reporte por rango de fecha";
            ListarUpas();
            _tipoArchivos = new ObservableCollection<NameDTO>()
            {
                new NameDTO()
                {
                    Id = Guid.NewGuid(),
                    Nombre = GrupoTipoArchivo.PDF.ToString(),
                },
                new NameDTO()
                {
                    Id = Guid.NewGuid(),
                    Nombre = GrupoTipoArchivo.EXCEL.ToString(),
                }
            };
            SendCommand = new Command(GenerateReportClicked);

        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("filtroFecha"))
            {
                FiltroFecha = parameters.GetValue<FiltroFechaModel>("filtroFecha");
            }
        }
        #region properties
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }

        public FiltroFechaModel FiltroFecha
        {
            get => _filtroFecha;
            set => SetProperty(ref _filtroFecha, value);
        }

        public ObservableCollection<NameDTO> TipoArchivos
        {
            get => _tipoArchivos;
            set => SetProperty(ref _tipoArchivos, value);
        }

        public NameDTO TipoArchivo
        {
            get => _tipoArchivo;
            set => SetProperty(ref _tipoArchivo, value);
        }

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }

        public NameDTO Upa
        {
            get => _upa;
            set => SetProperty(ref _upa, value);

        }
        public Command NavigatePopupFilterCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(FilterDatePopupPage));
                });
            }
        }

        public Command SendCommand { get; set; }


        #endregion

        private async void ListarUpas()
        {
         
            if (_isSuperAdmin)
            {
                UserDialogs.Instance.ShowLoading("Cargando...");
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                        Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                        Upas.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
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
        private async void GenerateReportClicked(object obj)
        {
            try
            {
                if (!this.Validations())
                {
                    await PopupNavigation.Instance.PushAsync(new MessagePopupPage("Todos los campos son requeridos.", Constants.ICON_WARNING));
                    return;
                }

                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                UserDialogs.Instance.ShowLoading("Enviando...");


                ReportFilterRequest reportFilterRequest = new ReportFilterRequest
                {
                    TipoFormato = TipoArchivo.Nombre == GrupoTipoArchivo.PDF.ToString() ? GrupoTipoArchivo.PDF : GrupoTipoArchivo.EXCEL,
                    UpaId = Upa.Id,
                    FechaInicial = _filtroFecha.FechaInicio,
                    FechaFinal = _filtroFecha.FechaFin
                };
                var response = await _apiService.PostAsyncWithToken(URL_API, "reports-laboratory/by-date", reportFilterRequest, TokenUser.Token);
                await PopupNavigation.Instance.PushAsync(new MessagePopupPage(response.MensajeHttp));
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

        private bool Validations()
        {

            bool isFiltroFechaValid = _filtroFecha != null;
            bool isUpaValid = Upa != null;
            bool istipoArchivoValid = TipoArchivo != null;

            if (TokenUser.User.RolId == (int)TipoRol.Administrador)
            {
                isUpaValid = true;
            }
            return isFiltroFechaValid && isUpaValid && istipoArchivoValid;
        }
    }
}
