using Acr.UserDialogs;
using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.CommonUtils.Constants;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes
{
    public class FilterDatePopupViewModel : BaseViewModel
    {
        private bool _isEnabled;
        private DateTime? _fechaInicial;
        private DateTime _finalMinimumDate;
        private DateTime? _fechaFinal;
        private TimeSpan? _horaInicial;
        private TimeSpan? _horaFinal;

        public FilterDatePopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            SaveCommand = new Command(SaveClicked);
            _horaInicial = new TimeSpan(2, 0, 0);
            _horaFinal = new TimeSpan(11, 0, 0);
        }
        public DateTime? DateInitial
        {
            get
            {
                if (_fechaInicial != null)
                {
                    IsEnabled = true;
                    DateFinal = new DateTime(_fechaInicial.Value.Year, _fechaInicial.Value.Month, _fechaInicial.Value.Day);
                    FinalMinimumDate = DateFinal.Value;
                }
                return _fechaInicial;
            }
            set => SetProperty(ref _fechaInicial, value);
        }

        public DateTime? DateFinal
        {
            get => _fechaFinal;
            set
            {
                SetProperty(ref _fechaFinal, value);
            }
        }

        public TimeSpan? SaveTimeInitial
        {
            get
            {
                return _horaInicial;
            }
            set => SetProperty(ref _horaInicial, value);
        }
        public TimeSpan? SaveTimeFinal
        {
            get => _horaFinal;
            set => SetProperty(ref _horaFinal, value);
        }

        public DateTime InitialMinimumDate => DateTime.Now.AddMonths(-6);

        public DateTime FinalMinimumDate
        {
            get => _finalMinimumDate;
            set => SetProperty(ref _finalMinimumDate, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public Command SaveCommand { get; set; }

        private async void SaveClicked(object obj)
        {
            try
            {
                var error = this.Validations();
                if (!error.valido)
                {
                    AlertWarning(error.mensaje);          
                    return;
                }
                UserDialogs.Instance.ShowLoading("Guardando...");
                var rangos = new FiltroFechaModel
                {
                    FechaInicio = new DateTime(_fechaInicial.Value.Year, _fechaInicial.Value.Month, _fechaInicial.Value.Day,
                    _horaInicial.Value.Hours, _horaInicial.Value.Minutes, 00),

                    FechaFin = new DateTime(_fechaFinal.Value.Year, _fechaFinal.Value.Month, _fechaFinal.Value.Day,
                    _horaFinal.Value.Hours, _horaFinal.Value.Minutes, 00),
                };
                UserDialogs.Instance.HideLoading();
                var parameters = new NavigationParameters
                {
                { "filtroFecha", rangos }
            };
                await _navigationService.GoBackAsync(parameters);
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
        }

        private (string mensaje, bool valido) Validations()
        {
            int rangoHoras = TimeSpan.Compare(_horaFinal.Value, _horaInicial.Value);
            var valorRangoFechas = (_fechaFinal.Value.Date - _fechaInicial.Value.Date).TotalHours;

            if (rangoHoras != 1 && valorRangoFechas <= 23)
                return ("La hora final tiene que ser mayor a la inicial.", false);

            return ("rango correcto", true);
        }
    }
}
