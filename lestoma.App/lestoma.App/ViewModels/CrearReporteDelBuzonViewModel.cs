using lestoma.CommonUtils.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace lestoma.App.ViewModels
{
    public class CrearReporteDelBuzonViewModel : BaseViewModel
    {
        #region Fields
        private readonly IApiService _apiService;
        private bool _isRunning;
        private bool _isEnabled;
        #endregion

        #region Constructor
        public CrearReporteDelBuzonViewModel(INavigationService navigationService, IApiService apiService)
              : base(navigationService)
        {
            _apiService = apiService;
            Title = "Main Page";
        }
        #endregion
    }
}
