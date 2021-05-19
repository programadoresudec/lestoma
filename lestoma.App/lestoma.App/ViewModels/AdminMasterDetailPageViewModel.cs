using lestoma.App.ItemViewModels;
using lestoma.App.Views;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Responses;
using Newtonsoft.Json;
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
        private UserApp _userApp;
        public AdminMasterDetailPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            _instance = this;
            _navigationService = navigationService;
            LoadMenus();
            LoadUser();
        }
        public ObservableCollection<MenuItemViewModel> Menus { get; set; }
        public static AdminMasterDetailPageViewModel GetInstance()
        {
            return _instance;
        }

        public UserApp UserApp
        {
            get => _userApp;
            set => SetProperty(ref _userApp, value);
        }

        public void LoadUser()
        {
            if (MovilSettings.IsLogin)
            {
                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(MovilSettings.Token);
                UserApp = token.User;
            }
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
                    Icon = "icon_signOut",
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
