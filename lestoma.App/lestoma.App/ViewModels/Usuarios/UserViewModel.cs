using lestoma.App.Models;
using lestoma.App.Views.Usuarios;
using lestoma.CommonUtils.Constants;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using Newtonsoft.Json;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Usuarios
{
    public class UserViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<InfoUserModel> _usuarios;
        private bool _isNavigating = false;
        public UserViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
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
                    await NavigationService.NavigateAsync($"{nameof(CreateOrEditUserPage)}");
                });
            }
        }

        public Command EditCommand { get; set; }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey(Constants.REFRESH))
            {
                ConsumoService();
            }
        }


        private bool CanNavigate(object arg)
        {
            return true;
        }

        private void LoadUsers()
        {
            if (!_apiService.CheckConnection())
            {
                AlertNoInternetConnection();
                return;
            }
            ConsumoService();
        }

        private async void UserSelected(object objeto)
        {

            if (!_isNavigating)
            {
                _isNavigating = true;
                Syncfusion.ListView.XForms.ItemTappedEventArgs lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;

                if (!(lista.ItemData is InfoUserModel infoUser))
                    return;

                string userEdit = JsonConvert.SerializeObject(infoUser.InfoUser);
                var parameters = new NavigationParameters
                    {
                        { "usuario", userEdit }
                    };
                await NavigationService.NavigateAsync($"{nameof(CreateOrEditUserPage)}", parameters);
                _isNavigating = false;
            }
        }

        private async void ConsumoService()
        {
            try
            {
                IsBusy = true;
                Users = new ObservableCollection<InfoUserModel>();

                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<InfoUserDTO>>(URL_API, "usuarios/listado", TokenUser.Token);
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
                            Users.Add(info);
                        }
                    }
                }
                else
                {
                    AlertWarning(response.MensajeHttp);
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
