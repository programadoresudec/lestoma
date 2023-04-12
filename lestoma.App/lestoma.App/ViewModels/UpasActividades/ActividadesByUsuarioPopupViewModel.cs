using Acr.UserDialogs;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests.Filters;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace lestoma.App.ViewModels.UpasActividades
{
    public class ActividadesByUsuarioPopupViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<object> _actividades;
        private UpaUserFilterRequest _filtro;
        public ActividadesByUsuarioPopupViewModel(INavigationService navigationService, IApiService apiService)
            : base(navigationService)
        {
            _apiService = apiService;
        }
        public UpaUserFilterRequest Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value);
        }
        public ObservableCollection<object> Actividades
        {
            get => _actividades;
            set => SetProperty(ref _actividades, value);
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("filtroUserUpa"))
            {
                Filtro = parameters.GetValue<UpaUserFilterRequest>("filtroUserUpa");
                LoadActivities(Filtro);
            }

        }

        private async void LoadActivities(UpaUserFilterRequest upaUserFilterRequest)
        {
            try
            {
                if (_apiService.CheckConnection())
                {
                    UserDialogs.Instance.ShowLoading("Cargando...");

                    string queryString = Reutilizables.GenerateQueryString(upaUserFilterRequest);
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                        $"detalle-upas-actividades/listar-por-usuario{queryString}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        var listado = (List<NameDTO>)response.Data;
                        Actividades = new ObservableCollection<object>();
                        foreach (var item in listado)
                        {
                            Actividades.Add(new
                            {
                                item.Nombre
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeeError(ex);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
    }
}
