using lestoma.App.Views;
using lestoma.App.Views.Usuarios;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class ModeOfflineViewModel : BaseViewModel
    {
        public ModeOfflineViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }

        public Command NavigateMenuOfflineCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync($"{nameof(MenuMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                });
            }
        }
    }
}
