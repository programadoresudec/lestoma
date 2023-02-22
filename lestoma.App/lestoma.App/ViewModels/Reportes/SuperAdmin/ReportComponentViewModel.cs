using lestoma.App.Views.Reportes;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes.SuperAdmin
{
    class ReportComponentViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private bool _isSuperAdmin;

        public ReportComponentViewModel(INavigationService navigationService, IApiService apiService) : base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador ? true : false;
            _apiService = apiService;
            Title = "Reporte por rango de fecha";
            ListarUpas();
        }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;

            set => SetProperty(ref _upas, value);
        }

        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }
        public NameDTO Upa
        {
            get => _upa;
            set => SetProperty(ref _upa, value);

        }
        public Command NavigatePopupFilterCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await _navigationService.NavigateAsync(nameof(FilterDatePopupPage));
                });
            }
        }
        private async void ListarUpas()
        {

            if (_isSuperAdmin)
            {
                try
                {
                    if (_apiService.CheckConnection())
                    {
                        ResponseDTO upas = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, "upas/listar-nombres", TokenUser.Token);
                        Upas = new ObservableCollection<NameDTO>((List<NameDTO>)upas.Data);
                        Upas.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                    }
                }
                catch (Exception ex)
                {
                    SeeError(ex);
                }
            }
        }

    }
}
