using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lestoma.App.Views
{
    public partial class MenuMasterDetailPage : MasterDetailPage
    {
        private readonly INavigationService _navigationService;

        public MenuMasterDetailPage(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InitializeComponent();
            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;
        }
        ~MenuMasterDetailPage()
        {
            Connectivity.ConnectivityChanged -= ConnectivityChangedHandler;
        }

        private void ConnectivityChangedHandler(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        if (int.Parse(MovilSettings.CountModeOnline) == 0)
                        {
                            var parameters = new NavigationParameters
                                {
                                    { "isModeOnline", true }
                                };
                            await _navigationService.NavigateAsync($"{nameof(MenuMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}", parameters);
                        }
                        else
                        {
                            await _navigationService.NavigateAsync($"{nameof(MenuMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                        }

                        var dato = int.Parse(MovilSettings.CountModeOnline) + 1;
                        MovilSettings.CountModeOnline = dato.ToString();
                    }
                    else
                    {
                        MovilSettings.CountModeOnline = "0";
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