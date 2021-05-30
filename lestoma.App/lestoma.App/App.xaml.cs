using lestoma.App.ViewModels;
using lestoma.App.Views;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Services;
using Prism;
using Prism.Ioc;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

[assembly: ExportFont("Montserrat-Bold.ttf", Alias = "Montserrat-Bold")]
[assembly: ExportFont("Montserrat-Medium.ttf", Alias = "Montserrat-Medium")]
[assembly: ExportFont("Montserrat-Regular.ttf", Alias = "Montserrat-Regular")]
[assembly: ExportFont("Montserrat-SemiBold.ttf", Alias = "Montserrat-SemiBold")]
[assembly: ExportFont("UIFontIcons.ttf", Alias = "FontIcons")]
namespace lestoma.App
{
    public partial class App
    {
        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.
               RegisterLicense("NDMxODc5QDMxMzkyZTMxMmUzMENoS0RZSTVCbThZYzBtd2tCQjFMb2xnbklkSkFwNXBlWXl3YlZpOE9mQmc9");
            InitializeComponent();

            if (!MovilSettings.IsLogin)
            {
                await NavigationService.NavigateAsync($"NavigationPage/{nameof(LoginPage)}");
            }
            else
            {
                await NavigationService.NavigateAsync($"{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(CrearReporteDelBuzonPage)}");
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.Register<IApiService, ApiService>();
            containerRegistry.Register<IFilesHelper, FilesHelper>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<RegistroPage, RegistroPageViewModel>();
            containerRegistry.RegisterForNavigation<AboutPage, AboutPageViewModel>();
            containerRegistry.RegisterForNavigation<ForgotPasswordPage, ForgotPasswordViewModel>();
            containerRegistry.RegisterForNavigation<AdminMasterDetailPage, AdminMasterViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<ResetPasswordPage, ResetPasswordViewModel>();
            containerRegistry.RegisterForNavigation<ChangePasswordPage, ChangePasswordViewModel>();
            containerRegistry.RegisterForNavigation<BuzonDeReportesPage, BuzonDeReportesViewModel>();
            containerRegistry.RegisterForNavigation<CrearReporteDelBuzonPage, CrearReporteDelBuzonViewModel>();
        }
    }
}
