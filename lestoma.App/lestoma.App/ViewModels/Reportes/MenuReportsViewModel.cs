using lestoma.App.ItemViewModels;
using lestoma.App.Views;
using lestoma.App.Views.Reportes;
using lestoma.App.Views.Reportes.SuperAdmin;
using lestoma.CommonUtils.Enums;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lestoma.App.ViewModels.Reportes
{
    public class MenuReportsViewModel : BaseViewModel
    {
        public ObservableCollection<MenuItemViewModel> MenuReport { get; set; }
        public MenuReportsViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            LoadMenu();
        }

        private void LoadMenu()
        {
            List<Menu> menu = new List<Menu>();

            if (Rol == (int)TipoRol.SuperAdministrador)
            {
                menu.Add(new Menu
                {
                    Icon = "icon_daily",
                    PageName = $"{nameof(ReportDailyPage)}",
                    Title = "Configurar reporte diario."
                });
            }
            menu.Add(new Menu
            {
                Icon = "icon_acuaponic",
                PageName = $"{nameof(ReportByDatePage)}",
                Title = "Generar reporte por fecha."
            });
            menu.Add(new Menu
            {
                Icon = "icon_acuaponic",
                PageName = Rol == (int)TipoRol.SuperAdministrador ? $"{nameof(AboutPage)}" : $"{nameof(AboutPage)}",
                Title = "Generar reporte por componentes."
            });

            MenuReport = new ObservableCollection<MenuItemViewModel>(
                 menu.Select(m => new MenuItemViewModel(_navigationService)
                 {
                     Icon = m.Icon,
                     PageName = m.PageName,
                     Title = m.Title,
                     IsLoginRequired = m.IsLoginRequired
                 }).ToList());
        }
    }
}
