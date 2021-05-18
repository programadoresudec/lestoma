using lestoma.App.ItemViewModels;
using lestoma.App.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lestoma.App.ViewModels
{
    public class AdminMasterDetailPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private static AdminMasterDetailPageViewModel _instance;
        public AdminMasterDetailPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            _instance = this;
            _navigationService = navigationService;
            LoadMenus();
        }
        public ObservableCollection<MenuItemViewModel> Menus { get; set; }
        public static AdminMasterDetailPageViewModel GetInstance()
        {
            return _instance;
        }
        private void LoadMenus()
        {
            List<Menu> menus = new List<Menu>
            {
                new Menu
                {
                    Icon = "icon_about",
                    PageName = $"{nameof(AboutPage)}",
                    Title = "acerca"
                },
                new Menu
                {
                    Icon = "icon_settings",
                    PageName = $"{nameof(SettingsPage)}",
                    Title ="Configuración"
                },
                 new Menu
                {
                    Icon = "icon_settings",
                    PageName = $"{nameof(LoginPage)}",
                    Title ="Cerrar Sesión"
                }

            };

            Menus = new ObservableCollection<MenuItemViewModel>(
                menus.Select(m => new MenuItemViewModel(_navigationService)
                {
                    Icon = m.Icon,
                    PageName = m.PageName,
                    Title = m.Title,
                    IsLoginRequired = m.IsLoginRequired
                }).ToList());
        }
    }
}
