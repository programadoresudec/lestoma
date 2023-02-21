using lestoma.App.Models;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes
{
    public class FilterDatePopupViewModel : BaseViewModel
    {
        private FiltroFechaModel _filtroDate;
        private DateTime _initialMinimumDate;
        private DateTime _finalMinimumDate;
        public FilterDatePopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            SaveCommand = new Command(SaveClicked);
            _filtroDate = new FiltroFechaModel()
            {
                FechaFinal = DateTime.Now,
                FechaInicial = DateTime.Now,
            };
            _initialMinimumDate = DateTime.Now.AddMonths(6);
            _finalMinimumDate = _filtroDate.FechaInicial.AddDays(1);
        }


        public FiltroFechaModel FiltroDateModel
        {
            get => _filtroDate;
            set => SetProperty(ref _filtroDate, value);
        }

        public DateTime InitialMinimumDate
        {
            get => _initialMinimumDate;
            set => SetProperty(ref _initialMinimumDate, value);
        }

        public DateTime FinalMinimumDate
        {
            get => _finalMinimumDate;
            set
            {
                _finalMinimumDate = FiltroDateModel.FechaInicial.AddDays(1);
                SetProperty(ref _finalMinimumDate, value);
            }
        }

        public Command SaveCommand { get; set; }

        private void SaveClicked(object obj)
        {

        }
    }
}
