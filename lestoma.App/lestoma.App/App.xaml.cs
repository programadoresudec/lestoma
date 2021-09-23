using lestoma.App.ViewModels.Actividades;
using lestoma.App.ViewModels.Account;
using lestoma.App.ViewModels.Buzon;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.App.Views.Actividades;
using lestoma.App.Views.Buzon;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Services;
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;
using System.IO;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using lestoma.App.ViewModels;
using lestoma.App.Views.Upas;
using lestoma.App.ViewModels.Upas;

[assembly: ExportFont("Montserrat-Bold.ttf", Alias = "Montserrat-Bold")]
[assembly: ExportFont("Montserrat-Medium.ttf", Alias = "Montserrat-Medium")]
[assembly: ExportFont("Montserrat-Regular.ttf", Alias = "Montserrat-Regular")]
[assembly: ExportFont("Montserrat-SemiBold.ttf", Alias = "Montserrat-SemiBold")]
[assembly: ExportFont("UIFontIcons.ttf", Alias = "FontIcons")]
namespace lestoma.App
{
    public partial class App
    {
        public static string DbPathSqlLite { get; set; } =
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "lestoma.db");

        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.
               RegisterLicense("NTAyMTU0QDMxMzkyZTMyMmUzMGFpUGh6YlNOa3dqb1R5ZEZ3OS9YN3NEQ3dpd2dmSW1zditpUGl5MXZHb289");
            InitializeComponent();

            if (!MovilSettings.IsLogin)
            {
                await NavigationService.NavigateAsync($"NavigationPage/{nameof(MandarTramar)}");
            }
            else
            {
                await NavigationService.NavigateAsync($"{nameof(AdminMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.Register<IApiService, ApiService>();
            containerRegistry.Register<IFilesHelper, FilesHelper>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterPopupNavigationService();
            containerRegistry.RegisterPopupDialogService();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<MandarTramar, MandarTramaViewModel>();
            containerRegistry.RegisterForNavigation<RegistroPage, RegistroPageViewModel>();
            containerRegistry.RegisterForNavigation<AboutPage, AboutPageViewModel>();
            containerRegistry.RegisterForNavigation<ForgotPasswordPage, ForgotPasswordViewModel>();
            containerRegistry.RegisterForNavigation<AdminMasterDetailPage, AdminMasterViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<ResetPasswordPage, ResetPasswordViewModel>();
            containerRegistry.RegisterForNavigation<ChangePasswordPage, ChangePasswordViewModel>();
            containerRegistry.RegisterForNavigation<BuzonDeReportesPage, BuzonDeReportesViewModel>();
            containerRegistry.RegisterForNavigation<CrearReporteDelBuzonPage, CrearReporteDelBuzonViewModel>();
            containerRegistry.RegisterForNavigation<SignOutPopupPage, SignOutPopupPageViewModel>();
            containerRegistry.RegisterForNavigation<LoadingPopupPage>();
            containerRegistry.RegisterForNavigation<ActividadPage, ActividadPageViewModel>();
            containerRegistry.RegisterForNavigation<CrearOrEditActividadPage, CrearOrEditActividadPageViewModel>();
            containerRegistry.RegisterForNavigation<UpaPage, UpaPageViewModel>();
        }
    }
}
