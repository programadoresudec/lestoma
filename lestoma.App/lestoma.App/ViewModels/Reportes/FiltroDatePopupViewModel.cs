using lestoma.App.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace lestoma.App.ViewModels.Reportes
{
    public class FiltroDatePopupViewModel : BaseViewModel
    {
        private FiltroFechaModel Filtro;
        public FiltroDatePopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }
    }
}
