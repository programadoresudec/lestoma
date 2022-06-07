using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lestoma.App.Views
{
    public partial class AdminMasterDetailPage : MasterDetailPage
    {
        private readonly INavigationService _navigationService;
        public AdminMasterDetailPage(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InitializeComponent();
            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;
        }

        private void ConnectivityChangedHandler(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await _navigationService.NavigateAsync($"{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                    }
                    else
                    {
                        await _navigationService.NavigateAsync(nameof(ModeOfflinePage));
                    }
                });
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
            }
        }

    }
}