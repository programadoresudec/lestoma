using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lestoma.App.ViewModels.Usuarios
{
    public class CreateOrEditUserViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        public CreateOrEditUserViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;

        }
    }
}
