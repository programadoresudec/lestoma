using lestoma.App.ViewModels;
using lestoma.App.ViewModels.Account;
using lestoma.App.ViewModels.Actividades;
using lestoma.App.ViewModels.Buzon;
using lestoma.App.ViewModels.Componentes;
using lestoma.App.ViewModels.Laboratorio;
using lestoma.App.ViewModels.Modulos;
using lestoma.App.ViewModels.Reportes;
using lestoma.App.ViewModels.Reportes.SuperAdmin;
using lestoma.App.ViewModels.Sincronizaciones;
using lestoma.App.ViewModels.Upas;
using lestoma.App.ViewModels.UpasActividades;
using lestoma.App.ViewModels.Usuarios;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.App.Views.Actividades;
using lestoma.App.Views.Buzon;
using lestoma.App.Views.Componentes;
using lestoma.App.Views.Laboratorio;
using lestoma.App.Views.Modulos;
using lestoma.App.Views.Reportes;
using lestoma.App.Views.Reportes.SuperAdmin;
using lestoma.App.Views.Sincronizaciones;
using lestoma.App.Views.Upas;
using lestoma.App.Views.UpasActividades;
using lestoma.App.Views.Usuarios;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Services;
using lestoma.DatabaseOffline.IConfiguration;
using Newtonsoft.Json;
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

[assembly: ExportFont("Montserrat-Bold.ttf", Alias = "Montserrat-Bold")]
[assembly: ExportFont("Montserrat-Medium.ttf", Alias = "Montserrat-Medium")]
[assembly: ExportFont("Montserrat-Regular.ttf", Alias = "Montserrat-Regular")]
[assembly: ExportFont("Montserrat-SemiBold.ttf", Alias = "Montserrat-SemiBold")]
[assembly: ExportFont("UIFontIcons.ttf", Alias = "FontIcons")]
[assembly: ExportFont("fontello.ttf", Alias = "Fontello")]
namespace lestoma.App
{
    public partial class App
    {
        public static string DbPathSqlLite { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "lestoma.db");

        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.
               RegisterLicense("NTI5MzU5QDMxMzkyZTMzMmUzMGZ5cEJxMUFDNHhqS0hEVlVHU3NCTHNsUTNGOGpEM015bjVJQ05hUkpXOWM9");
            InitializeComponent();
            if (VersionTracking.IsFirstLaunchEver)
            {
                await NavigationService.NavigateAsync($"NavigationPage/{nameof(OnBoardingPage)}");
            }
            else
            {
                if (!MovilSettings.IsLogin)
                {
                    await NavigationService.NavigateAsync($"NavigationPage/{nameof(LoginPage)}");
                }
                else
                {
                    TokenDTO TokenUser = !string.IsNullOrEmpty(MovilSettings.Token)
                        ? JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token) : null;

                    if (TokenUser != null)
                    {
                        if (TokenUser.Expiration <= DateTime.Now)
                        {
                            MovilSettings.Token = null;
                            MovilSettings.IsLogin = false;
                            await NavigationService.NavigateAsync($"NavigationPage/{nameof(LoginPage)}");
                        }
                        else
                        {
                            await NavigationService.NavigateAsync($"{nameof(MenuMasterDetailPage)}/NavigationPage/{nameof(AboutPage)}");
                        }
                    }
                    else
                    {
                        await NavigationService.NavigateAsync($"NavigationPage/{nameof(LoginPage)}");
                    }

                }
            }

        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.Register<IApiService, ApiService>();
            containerRegistry.Register<IFilesHelper, FilesHelper>();

            #region injection UnitOfwork Database OFfline
            containerRegistry.Register<IUnitOfWork, UnitOfWork>();
            #endregion

            #region Navegaciones
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterPopupNavigationService();
            containerRegistry.RegisterPopupDialogService();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<MandarTramar, MandarTramaViewModel>();
            containerRegistry.RegisterForNavigation<RegistroPage, RegistroViewModel>();
            containerRegistry.RegisterForNavigation<AboutPage, AboutPageViewModel>();
            containerRegistry.RegisterForNavigation<ForgotPasswordPage, ForgotPasswordViewModel>();
            containerRegistry.RegisterForNavigation<MenuMasterDetailPage, MenuMasterViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
            containerRegistry.RegisterForNavigation<ResetPasswordPage, ResetPasswordViewModel>();
            containerRegistry.RegisterForNavigation<ChangePasswordPage, ChangePasswordViewModel>();
            containerRegistry.RegisterForNavigation<BuzonDeReportesPage, BuzonDeReportesViewModel>();
            containerRegistry.RegisterForNavigation<CrearReporteDelBuzonPage, CrearReporteDelBuzonViewModel>();
            containerRegistry.RegisterForNavigation<SignOutPopupPage, SignOutPopupViewModel>();
            containerRegistry.RegisterForNavigation<LoadingPopupPage>();
            containerRegistry.RegisterForNavigation<SetPointPage, SetPointViewModel>();
            containerRegistry.RegisterForNavigation<LecturaSensorPage, LecturaSensorViewModel>();
            containerRegistry.RegisterForNavigation<EstadoActuadorPage, EstadoActuadorViewModel>();
            containerRegistry.RegisterForNavigation<ActividadPage, ActividadViewModel>();
            containerRegistry.RegisterForNavigation<CrearOrEditActividadPage, CrearOrEditActividadViewModel>();
            containerRegistry.RegisterForNavigation<UpaPage, UpaViewModel>();
            containerRegistry.RegisterForNavigation<DetalleUpaActividadPage, DetalleUpaActividadViewModel>();
            containerRegistry.RegisterForNavigation<CreateOrEditUpaPage, CreateOrEditUpaViewModel>();
            containerRegistry.RegisterForNavigation<CreateOrEditDetalleUpaActividadPage, CreateOrEditDetalleUpaActividadViewModel>();
            containerRegistry.RegisterForNavigation<OnBoardingPage, OnBoardingViewModel>();
            containerRegistry.RegisterForNavigation<ModeOfflinePage, ModeOfflineViewModel>();
            containerRegistry.RegisterForNavigation<ModuloPage, ModuloViewModel>();
            containerRegistry.RegisterForNavigation<CreateOrEditModuloPage, CreateOrEditModuloViewModel>();
            containerRegistry.RegisterForNavigation<MenuReportsPage, MenuReportsViewModel>();
            containerRegistry.RegisterForNavigation<ReportDailyPage, ReportDailyViewModel>();
            containerRegistry.RegisterForNavigation<ReportByDatePage, ReportByDateViewModel>();
            containerRegistry.RegisterForNavigation<ActividadesByUsuarioPopupPage, ActividadesByUsuarioPopupViewModel>();
            containerRegistry.RegisterForNavigation<UserPage, UserViewModel>();
            containerRegistry.RegisterForNavigation<CreateOrEditUserPage, CreateOrEditUserViewModel>();
            containerRegistry.RegisterForNavigation<MoreInfoPopupPage, MoreInfoPopupViewModel>();
            containerRegistry.RegisterForNavigation<ComponentPage, ComponentViewModel>();
            containerRegistry.RegisterForNavigation<CreateOrEditComponentPage, CreateOrEditComponentViewModel>();
            containerRegistry.RegisterForNavigation<InfoEstadoPopupPage, InfoEstadoPopupViewModel>();
            containerRegistry.RegisterForNavigation<MessagePopupPage>();
            containerRegistry.RegisterForNavigation<DashboardHangFirePage, DashboardHangfireViewModel>();
            containerRegistry.RegisterForNavigation<ModulosUpaPage, ModulosUpaViewModel>();
            containerRegistry.RegisterForNavigation<ComponentesModuloPage, ComponentesModuloViewModel>();
            containerRegistry.RegisterForNavigation<SyncronizarDataPopupPage, SyncronizarDataViewModel>();
            containerRegistry.RegisterForNavigation<FilterDatePopupPage, FilterDatePopupViewModel>();
            containerRegistry.RegisterForNavigation<ReporteComponentPage, ReportComponentViewModel>();
            containerRegistry.RegisterForNavigation<InfoProtocolPopupPage, InfoProtocolPopupViewModel>();
            containerRegistry.RegisterForNavigation<CreateEditProtocolPopupPage, CreateEditProtocolPopupViewModel>();
            #endregion      
        }
    }
}
