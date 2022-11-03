using lestoma.App.Views;
using lestoma.App.Views.Upas;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Plugin.Toast;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class UpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<UpaDTO> _upas;
        public UpaViewModel(INavigationService navigationService, IApiService apiService) :
            base(navigationService)
        {
            _apiService = apiService;
            _upas = new ObservableCollection<UpaDTO>();
            EditCommand = new Command<object>(UpaSelected, CanNavigate);
            LoadUpas();
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }

        public ObservableCollection<UpaDTO> Upas
        {
            get => _upas;
            set => SetProperty(ref _upas, value);
        }
        public UpaDTO ItemDelete { get; set; }

        public Command EditCommand { get; set; }
        public Command AddCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(CreateOrEditUpaPage), null, useModalNavigation: true, true);
                });
            }
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("refresh"))
            {
                ConsumoService(true);
            }
        }
        private async void UpaSelected(object objeto)
        {
            var lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var upa = lista.ItemData as UpaDTO;

            if (upa == null)
                return;

            UpaRequest upaEdit = new UpaRequest
            {
                CantidadActividades = upa.CantidadActividades,
                Descripcion = upa.Descripcion,
                Id = upa.Id,
                Nombre = upa.Nombre
            };
            var parameters = new NavigationParameters
            {
                { "upa", upaEdit }
            };
            await _navigationService.NavigateAsync(nameof(CreateOrEditUpaPage), parameters, useModalNavigation: true, true);

        }


        private void LoadUpas()
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

        public async void DeleteClicked()
        {
            if (ItemDelete != null)
            {
                try
                {
                    if (!_apiService.CheckConnection())
                    {
                        CrossToastPopUp.Current.ShowToastWarning("No tiene internet por favor active el wifi.");
                        return;
                    }
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));
                    ResponseDTO response = await _apiService.DeleteAsyncWithToken(URL_API,
                        "upas", ItemDelete.Id, TokenUser.Token);
                    if (!response.IsExito)
                    {
                        CrossToastPopUp.Current.ShowToastError("Error " + response.MensajeHttp);
                        return;
                    }
                    CrossToastPopUp.Current.ShowToastSuccess(response.MensajeHttp);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    await _navigationService.ClearPopupStackAsync();
                }
            }
        }
        private async void ConsumoService(bool refresh = false)
        {
            try
            {
                if (!refresh)
                    await _navigationService.NavigateAsync(nameof(LoadingPopupPage));

                Upas = new ObservableCollection<UpaDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<UpaDTO>>(URL_API,
                    $"upas/listado", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<UpaDTO>)response.Data;
                    Upas = new ObservableCollection<UpaDTO>(listado);
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
                if (!refresh)
                    if (PopupNavigation.Instance.PopupStack.Any())
                        await PopupNavigation.Instance.PopAsync();
            }
        }
    }
}
