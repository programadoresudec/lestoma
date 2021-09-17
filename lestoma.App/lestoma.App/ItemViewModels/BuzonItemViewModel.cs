using lestoma.CommonUtils.DTOs;
using Prism.Commands;
using Prism.Navigation;

namespace lestoma.App.ItemViewModels
{
    public class BuzonItemViewModel : BuzonDTO
    {
        private readonly INavigationService _navigationService;

        private DelegateCommand _selectBuzonCommand;
        public BuzonItemViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        public DelegateCommand SelectProductCommand => _selectBuzonCommand ??
          (_selectBuzonCommand = new DelegateCommand(SelectBuzonAsync));

        private void SelectBuzonAsync()
        {
            NavigationParameters parameters = new NavigationParameters
            {
                { "product", this }
            };
        }
    }
}
