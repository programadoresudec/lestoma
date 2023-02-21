using lestoma.App.Views.Reportes;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes
{
    public class ReportByDateViewModel : BaseViewModel
    {
        public ReportByDateViewModel(INavigationService navigation)
            : base(navigation)
        {
            Title = "Reporte por rango de fecha";
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
    }
}
