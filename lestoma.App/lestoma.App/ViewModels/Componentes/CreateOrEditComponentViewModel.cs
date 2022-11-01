using lestoma.App.Models;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace lestoma.App.ViewModels.Componentes
{
    public class CreateOrEditComponentViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        public CreateOrEditComponentViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
          
        }
    }
}
