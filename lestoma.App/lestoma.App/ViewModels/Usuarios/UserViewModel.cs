﻿using lestoma.App.Models;
using lestoma.App.Views;
using lestoma.App.Views.Upas;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Usuarios
{
    public class UserViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<InfoUserModel> _usuarios;
        public UserViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            EditCommand = new Command<object>(UserSelected, CanNavigate);
            LoadUsers();
        }



        public ObservableCollection<InfoUserModel> Users
        {
            get => _usuarios;
            set => SetProperty(ref _usuarios, value);
        }

        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(CreateOrEditUserViewModel), null, useModalNavigation: true, true);
                });
            }
        }

        public Command EditCommand { get; set; }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            ConsumoService(true);
        }


        private bool CanNavigate(object arg)
        {
            return true;
        }

        private void LoadUsers()
        {
            if (_apiService.CheckConnection())
            {
                ConsumoService();
            }
            else
            {
                AlertNoInternetConnection();
            }
        }

        private async void UserSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var infoUser = lista.ItemData as InfoUserModel;

            if (infoUser == null)
                return;

            InfoUserDTO userEdit = infoUser.InfoUser;
            var parameters = new NavigationParameters
            {
                { "usuario", userEdit }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditUpaPage), parameters, useModalNavigation: true, true);

        }

        private async void ConsumoService(bool refresh = false)
        {
            try
            {
                Users = new ObservableCollection<InfoUserModel>();
                if (!refresh)
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));

                Response response = await _apiService.GetListAsyncWithToken<List<ActividadDTO>>(URL, "usuarios/listado", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<InfoUserDTO>)response.Data;
                    if (listado.Count > 0)
                    {
                        foreach (var item in listado)
                        {
                            InfoUserModel info = new InfoUserModel()
                            {
                                InfoUser = item
                            };
                        }
                    }
                }
                else
                {
                    AlertWarning(response.Mensaje);
                }
            }
            catch (Exception ex)
            {
                if (!refresh)
                    if (PopupNavigation.Instance.PopupStack.Any())
                        await PopupNavigation.Instance.PopAsync();
                SeeError(ex);
            }
        }

    }
}
