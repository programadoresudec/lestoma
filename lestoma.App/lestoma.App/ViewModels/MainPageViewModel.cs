﻿using lestoma.CommonUtils.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lestoma.App.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService,apiService)
        {
            Title = "Main Page";
        }
    }
}
