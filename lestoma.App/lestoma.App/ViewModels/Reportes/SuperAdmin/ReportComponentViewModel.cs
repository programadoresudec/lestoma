﻿using lestoma.App.Views.Reportes;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Reportes.SuperAdmin
{
    class ReportComponentViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private NameDTO _upa;
        private ObservableCollection<NameDTO> _upas;
        private ObservableCollection<NameDTO> _Components;
        private bool _isSuperAdmin;
        private bool _isEnabled;

        public ReportComponentViewModel(INavigationService navigationService, IApiService apiService) : base(navigationService)
        {
            _isSuperAdmin = TokenUser.User.RolId == (int)TipoRol.SuperAdministrador;
            _apiService = apiService;
            Title = "Reporte por rango de fecha";
            _Components = new ObservableCollection<NameDTO>();
            ListarUpas();
        }
        public ObservableCollection<NameDTO> Upas
        {
            get => _upas;

            set => SetProperty(ref _upas, value);
        }
        public ObservableCollection<NameDTO> Components
        {
            get => _Components;

            set => SetProperty(ref _Components, value);
        }
        public bool IsSuperAdmin
        {
            get => _isSuperAdmin;
            set => SetProperty(ref _isSuperAdmin, value);
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        public NameDTO Upa
        {
            get => _upa;
            set
            {
                SetProperty(ref _upa, value);
                ListarComponentes(_upa.Id);
            }

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
        private async void ListarComponentes(Guid IdUpa)
        {
            IsBusy = true;
            IsEnabled = true;
            try
            {
                if (_apiService.CheckConnection())
                {
                    Components.Clear();
                    ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API, $"componentes/listar-nombres-por-upa/{IdUpa}", TokenUser.Token);
                    if (response.IsExito)
                    {
                        Components = new ObservableCollection<NameDTO>((List<NameDTO>)response.Data);
                        Components.Insert(0, new NameDTO { Id = Guid.Empty, Nombre = "Todas" });
                    }
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
