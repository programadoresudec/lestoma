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
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes
{
    public class ReportByDateViewModel : BaseViewModel
    {
        #region attributes
        private readonly IApiService _apiService;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private NameArchivoDTO _tipoArchivo;
        private ObservableCollection<NameArchivoDTO> _tipoArchivos;
        private bool _isSuperAdmin;
        private FiltroFechaModel _filtroFecha;
        #endregion

        #region constructor
        public ReportByDateViewModel(INavigationService navigation, IApiService apiService)
           : base(navigation)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            Title = "Reporte por rango de fecha";
            ListarUpas();
            ListarTiposFormato();
            SendCommand = new Command(GenerateReportClicked);

        }
        #endregion

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

        public ObservableCollection<NameArchivoDTO> TipoArchivos
        {
            get => _tipoArchivos;
            set => SetProperty(ref _tipoArchivos, value);
        }

        public NameArchivoDTO TipoArchivo
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
                    await NavigationService.NavigateAsync(nameof(FilterDatePopupPage));
                });
            }
        }

        public Command SendCommand { get; set; }


        #endregion

        #region Methods
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("filtroFecha"))
            {
                FiltroFecha = parameters.GetValue<FiltroFechaModel>("filtroFecha");
            }
        }
        private void ListarTiposFormato()
        {
            _tipoArchivos = new ObservableCollection<NameArchivoDTO>()
            {
                new NameArchivoDTO()
                {
                    Id = (int)GrupoTipoArchivo.PDF,
                    Nombre = GrupoTipoArchivo.PDF.ToString(),
                },
                 new NameArchivoDTO()
                {
                    Id = (int)GrupoTipoArchivo.CSV,
                    Nombre = GrupoTipoArchivo.CSV.ToString(),
                },
                new NameArchivoDTO()
                {
                    Id = (int)GrupoTipoArchivo.EXCEL,
                    Nombre = GrupoTipoArchivo.EXCEL.ToString(),
                }
            };
        }
        private async void ListarUpas()
        {
            UserDialogs.Instance.ShowLoading("Cargando...");
            try
            {
                if (_apiService.CheckConnection())
                {
                    ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                    Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                    Upas.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                    if (!IsSuperAdmin)
                    {
                        ResponseDTO response = await _apiService.GetAsyncWithToken(URL_API, "usuarios/upa-asignada", TokenUser.Token);
                        if (response.IsExito)
                        {
                            var upa = ParsearData<NameDTO>(response);
                            var selected = Upas.Where(x => x.Id == upa.Id).FirstOrDefault();
                            Upa = selected;
                        }
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
                    TipoFormato = GetFormatFile(TipoArchivo.Id),
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

        private GrupoTipoArchivo GetFormatFile(int idFormato)
        {
            switch (idFormato)
            {
                case (int)GrupoTipoArchivo.PDF:
                    return GrupoTipoArchivo.PDF;
                case (int)GrupoTipoArchivo.CSV:
                    return GrupoTipoArchivo.CSV;
                case (int)GrupoTipoArchivo.EXCEL:
                    return GrupoTipoArchivo.EXCEL;
                default:
                    return GrupoTipoArchivo.PDF;
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
        #endregion
    }
}
