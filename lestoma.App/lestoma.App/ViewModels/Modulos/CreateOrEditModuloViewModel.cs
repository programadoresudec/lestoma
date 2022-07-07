using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;

namespace lestoma.App.ViewModels.Modulos
{
    public class CreateOrEditModuloViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        public CreateOrEditModuloViewModel(INavigationService navigationService, IApiService apiService) :
             base(navigationService)
        {
            _apiService = apiService;
        }
    }
}
