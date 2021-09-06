using lestoma.App.Views;
using lestoma.CommonUtils.Helpers;
using Prism.Commands;
using Prism.Navigation;

namespace lestoma.App.ItemViewModels
{
    public class MenuItemViewModel : Menu
    {
        private readonly INavigationService _navigationService;
        private DelegateCommand _selectMenuCommand;

        public MenuItemViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public DelegateCommand SelectMenuCommand => _selectMenuCommand ?? (_selectMenuCommand = new DelegateCommand(SelectMenuAsync));

        private async void SelectMenuAsync()
        {
            if (PageName == nameof(LoginPage) && MovilSettings.IsLogin)
            {
                MovilSettings.Token = null;
                MovilSettings.IsLogin = false;
            }

            if (IsLoginRequired && !MovilSettings.IsLogin)
            {
                NavigationParameters parameters = new NavigationParameters
                {
                    { "pageReturn", PageName }
                };

                await _navigationService.NavigateAsync($"/{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(LoginPage)}", parameters);
            }
            else
            {
                await _navigationService.NavigateAsync($"/{nameof(AdminMasterDetailPage)}/NavigationPage/{PageName}");
            }
        }
    }
    public class Menu
    {
        public string Icon { get; set; }

        public string Title { get; set; }

        public string PageName { get; set; }

        public bool IsLoginRequired { get; set; }
    }
}

