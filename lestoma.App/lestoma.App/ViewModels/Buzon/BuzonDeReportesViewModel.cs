using Acr.UserDialogs;
using lestoma.App.Views.Buzon;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Buzon
{
    public class BuzonDeReportesViewModel : BaseViewModel
    {
        #region attributes
        private readonly IApiService _apiService;
        private ObservableCollection<BuzonDTO> _reportesDelBuzon;
        private Command<object> itemTap;
        #endregion
        public BuzonDeReportesViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
            Title = "Buzón de reportes";
            SeeMoreInfoCommand = new Command<object>(OnSeeMoreInfo, CanNavigate);
            MoreReportsCommand = new Command(LoadMoreBuzonDeReportes);
            EditStatusCommand = new Command<object>(OnEditStatus, CanNavigate);
            _reportesDelBuzon = new ObservableCollection<BuzonDTO>();
            LoadBuzonDeReportes();
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                this.Page = 1;
                LoadBuzonDeReportes(true);
            }
        }
        #region properties
        public ObservableCollection<BuzonDTO> ReportesDelBuzon
        {
            get => _reportesDelBuzon;
            set => SetProperty(ref _reportesDelBuzon, value);
        }
        public Command MoreReportsCommand { get; set; }
        public Command EditStatusCommand { get; set; }
        public Command<object> SeeMoreInfoCommand
        {
            get => itemTap;
            set => SetProperty(ref itemTap, value);
        }
        #endregion

        #region methods
        private bool CanNavigate(object arg)
        {
            return true;
        }
        private async void OnSeeMoreInfo(object obj)
        {
            BuzonDTO buzon = (BuzonDTO)obj;
            if (buzon == null)
                return;
            var parameters = new NavigationParameters
            {
                { "BuzonId",  buzon.Id}
            };
            await NavigationService.NavigateAsync($"{nameof(MoreInfoPopupPage)}", parameters);
        }

        private async void OnEditStatus(object obj)
        {
            BuzonDTO buzon = (BuzonDTO)obj;
            if (buzon == null)
                return;

            if (buzon.EstadoId == (int)TipoEstadoBuzon.Escalado && TokenUser.User.RolId == (int)TipoRol.Administrador)
            {
                AlertWarning("El buzón ya ha sido escalado al super administrador.");
            }
            else if (TokenUser.User.RolId == (int)TipoRol.SuperAdministrador)
            {
                EditStatus(buzon.Id);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { "BuzonId",  buzon.Id}
                };
                await NavigationService.NavigateAsync($"{nameof(EditStatusPopupPage)}", parameters);
            }
        }

        private async void EditStatus(int buzonId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Editando...");
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                EditarEstadoBuzonRequest Request = new EditarEstadoBuzonRequest
                {
                    EstadoBuzon = new EstadoBuzonDTO()
                    {
                        Id = (int)TipoEstadoBuzon.Finalizado,
                        Nombre = EnumConfig.GetDescription(TipoEstadoBuzon.Finalizado)
                    },
                    BuzonId = buzonId,
                };

                ResponseDTO respuesta = await _apiService.PutAsyncWithToken(URL_API, "buzon-de-reportes/edit-status", Request, TokenUser.Token);
                if (!respuesta.IsExito)
                {
                    AlertWarning(respuesta.MensajeHttp);
                    return;
                }
                AlertSuccess(respuesta.MensajeHttp);
                this.Page = 1;
                this.LoadBuzonDeReportes(true);
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

        private async void LoadBuzonDeReportes(bool isEdit = false)
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                if (isEdit)
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");
                }
                else
                {
                    IsBusy = true;
                }
                var paginacion = new Paginacion
                {
                    Page = this.Page,
                    PageSize = this.PageSize
                };
                string querystring = Reutilizables.GenerateQueryString(paginacion);
                ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<BuzonDTO>(URL_API, $"buzon-de-reportes/paginar{querystring}", TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertError(response.MensajeHttp);
                    return;
                }
                Paginador<BuzonDTO> paginador = (Paginador<BuzonDTO>)response.Data;
                if (paginador.TotalDatos == 0)
                {
                    AlertWarning("No hay reportes en el buzón.");
                    return;
                }
                IsRefreshing = paginador.HasNextPage;
                ReportesDelBuzon = new ObservableCollection<BuzonDTO>(paginador.Datos);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                IsBusy = false;
                if (isEdit)
                {
                    UserDialogs.Instance.HideLoading();
                }
            }
        }
        private async void LoadMoreBuzonDeReportes()
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                UserDialogs.Instance.ShowLoading("Cargando...");
                if (_reportesDelBuzon.Any())
                {
                    var paginacion = new Paginacion
                    {
                        Page = this.Page,
                        PageSize = this.PageSize
                    };
                    string querystring = Reutilizables.GenerateQueryString(paginacion);
                    ResponseDTO response = await _apiService.GetPaginadoAsyncWithToken<BuzonDTO>(URL_API, $"buzon-de-reportes/paginar{querystring}", TokenUser.Token);
                    if (!response.IsExito)
                    {
                        AlertError(response.MensajeHttp);
                        return;
                    }
                    Paginador<BuzonDTO> paginador = (Paginador<BuzonDTO>)response.Data;
                    IsRefreshing = paginador.HasNextPage;
                    foreach (var item in paginador.Datos)
                    {
                        ReportesDelBuzon.Add(item);
                    }
                }

            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                Page++;
                UserDialogs.Instance.HideLoading();
            }
        }
        #endregion
    }
}
