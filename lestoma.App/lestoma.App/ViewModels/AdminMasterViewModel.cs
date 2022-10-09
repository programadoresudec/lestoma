﻿using lestoma.App.ItemViewModels;
using lestoma.App.Views;
using lestoma.App.Views.Account;
using lestoma.App.Views.Actividades;
using lestoma.App.Views.Buzon;
using lestoma.App.Views.Modulos;
using lestoma.App.Views.Reportes;
using lestoma.App.Views.Upas;
using lestoma.App.Views.UpasActividades;
using lestoma.App.Views.Usuarios;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using Newtonsoft.Json;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lestoma.App.ViewModels
{
    public class AdminMasterViewModel : BaseViewModel
    {
        private static AdminMasterViewModel _instance;
        private UserDTO _userApp;
        public AdminMasterViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _instance = this;
            LoadUser();
            LoadMenus();
        }
        public ObservableCollection<MenuItemViewModel> Menus { get; set; }
        public static AdminMasterViewModel GetInstance()
        {
            return _instance;
        }

        public UserDTO UserApp
        {
            get => _userApp;
            set => SetProperty(ref _userApp, value);
        }

        public void LoadUser()
        {
            if (MovilSettings.IsLogin)
            {
                TokenDTO token = JsonConvert.DeserializeObject<TokenDTO>(MovilSettings.Token);
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
                    Title = "acerca de lestoma"
                },

                new Menu
                {
                    Icon = "icon_acuaponic",
                    PageName = UserApp.RolId == (int)TipoRol.Administrador ? $"{nameof(AboutPage)}" : $"{nameof(AboutPage)}",
                    Title = "Laboratorio lestoma"
                },
                new Menu
                {
                    Icon = UserApp.RolId == (int)TipoRol.Administrador ? "icon_buzon" : "icon_crear_reporte",
                    PageName = UserApp.RolId == (int)TipoRol.Administrador || UserApp.RolId == (int)TipoRol.SuperAdministrador
                    ? $"{nameof(BuzonDeReportesPage)}" : $"{nameof(CrearReporteDelBuzonPage)}",
                    Title = UserApp.RolId == (int)TipoRol.Administrador || UserApp.RolId == (int)TipoRol.SuperAdministrador  ? "Buzon De Reportes"
                    : "Crear Reporte"
                },

            };

            if (UserApp.RolId == (int)TipoRol.SuperAdministrador)
            {
                List<Menu> menuSuperAdmin = new List<Menu>
                {
                    new Menu
                    {
                        Icon =  "icon_users",
                        PageName = $"{nameof(UserPage)}",
                        Title = "Usuarios"
                    },
                    new Menu
                    {
                        Icon =  "icon_actividad",
                        PageName = $"{nameof(ActividadPage)}",
                        Title = "Actividades"
                    },
                     new Menu
                    {
                        Icon =  "icon_module",
                        PageName = $"{nameof(ModuloPage)}",
                        Title = "Modulos"
                    },
                      new Menu
                    {
                        Icon =  "",
                        PageName = $"{nameof(MandarTramar)}",
                        Title = "Mandar trama"
                    },
                    new Menu
                    {
                        Icon = "icon_upa",
                        PageName = $"{nameof(UpaPage)}",
                        Title = "Upas"
                    },
                     new Menu
                    {
                        Icon =  "icon_detalles",
                        PageName = $"{nameof(DetalleUpaActividadPage)}",
                        Title = "Detalle de upas con actividades"
                    },
                     new Menu
                    {
                        Icon = "icon_generate_report",
                        PageName = $"{nameof(MenuReportsPage)}",
                        Title = "Generación de reportes"
                    }
                };
                menus.AddRange(menuSuperAdmin);
            }

            else if (UserApp.RolId == (int)TipoRol.Administrador)
            {
                menus.Add(new Menu
                {
                    Icon = "icon_generate_report",
                    PageName = $"{nameof(MenuReportsPage)}",
                    Title = "Generación de reportes"
                });
            }

            Menu settings = new Menu
            {
                Icon = "icon_settings",
                PageName = $"{nameof(SettingsPage)}",
                Title = "Configuración"
            };
            menus.Add(settings);
            Menu signout = new Menu
            {
                Icon = "icon_signOut",
                PageName = $"{nameof(SignOutPopupPage)}",
                Title = "Cerrar Sesión"
            };
            menus.Add(signout);


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
