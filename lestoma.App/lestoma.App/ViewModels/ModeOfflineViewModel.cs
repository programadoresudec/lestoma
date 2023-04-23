using lestoma.App.Views;
using lestoma.App.Views.Usuarios;
using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
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
                    if (MovilSettings.IsOnSyncToDevice)
                    {
                        await NavigationService.NavigateAsync($"{nameof(MenuMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                    }
                    else
                    {
                        await PopupNavigation.Instance.PushAsync(new MessagePopupPage("No puede ingresar al menú porque no sincronizó los datos necesarios para modo Offline."));
                    }
                });
            }
        }
    }
}
