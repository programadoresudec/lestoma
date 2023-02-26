using Acr.UserDialogs;
using lestoma.App.Views;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes.SuperAdmin
{
    public class ReportDailyViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private TimeSpan? _time;
        private bool _isVisible;
        public ReportDailyViewModel(INavigationService navigation, IApiService apiService) : base(navigation)
        {
            _apiService = apiService;
            SaveCommand = new Command(AsignReportDailyClicked);
            LoadTime();
        }


        public Command SaveCommand { get; set; }


        public TimeSpan? TimeForReportDaily
        {
            get => _time;
            set => SetProperty(ref _time, value);

        }
        public bool Isvisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);

        }

        private async void LoadTime()
        {
            try
            {
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                UserDialogs.Instance.ShowLoading("Cargando...");
                var response = await _apiService.GetAsyncWithToken(URL_API, "reports-laboratory/get-daily-time", TokenUser.Token);
                if (!response.IsExito)
                {
                    return;
                }

                TimeJobDTO time = ParsearData<TimeJobDTO>(response);
                TimeForReportDaily = time.Time;
                Title = $"La tarea recurrente ya esta configurada con la hora: {time.Time}";
                Isvisible = true;
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

        private async void AsignReportDailyClicked(object obj)
        {
            try
            {
                if (_time == null)
                {
                    AlertWarning("Seleccione una hora.");
                    return;
                }
                if (!_apiService.CheckConnection())
                {
                    AlertNoInternetConnection();
                    return;
                }
                UserDialogs.Instance.ShowLoading("Guardando...");
                ReportDailyFilterRequest request = new ReportDailyFilterRequest
                {
                    Hour = _time.Value.Hours,
                    Minute = _time.Value.Minutes,
                };
                var response = await _apiService.PostAsyncWithToken(URL_API, "reports-laboratory/daily", request, TokenUser.Token);
                if (!response.IsExito)
                {
                    AlertError(response.MensajeHttp);
                    return;
                }
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
    }
}
