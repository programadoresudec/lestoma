using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateOrEditUpaViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;

        public Command CreateOrEditCommand { get; }

        public CreateOrEditUpaViewModel(INavigationService navigationService, IApiService apiService)
           : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;

            CreateOrEditCommand = new Command(CreateOrEditarClicked);
        }

        private void CreateOrEditarClicked(object obj)
        {
            throw new NotImplementedException();
        }

        private void CargarDatos()
        {
            
        }
    }
}
