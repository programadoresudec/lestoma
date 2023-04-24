﻿using lestoma.App.Views.Laboratorio;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.DatabaseOffline.IConfiguration;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class ModulosUpaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private ObservableCollection<NameDTO> _modulos;
        private bool _isCheckConnection;
        private readonly IUnitOfWork _unitOfWork;
        private bool _isNavigating = false;
        public ModulosUpaViewModel(INavigationService navigationService, IApiService apiService, IUnitOfWork unitOfWork) :
             base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            Title = "Seleccione un modulo";
            _modulos = new ObservableCollection<NameDTO>();
            SeeComponentCommand = new Command<object>(ModuloSelected, CanNavigate);
            LoadModulos();
            MessageHelp = "Seleccione un modulo para ver los componentes correspondientes.\n\n Dentro de los tres puntos en la parte derecha superior podrá prender el bluetooth y hacer la conexión con el laboratorio.";
        }

        private bool CanNavigate(object arg)
        {
            return true;
        }

        public ObservableCollection<NameDTO> Modulos
        {
            get => _modulos;
            set => SetProperty(ref _modulos, value);
        }

        public bool IsCheckConnection
        {
            get => _isCheckConnection;
            set => SetProperty(ref _isCheckConnection, value);
        }

        public Command SeeComponentCommand { get; set; }

        private async void ModuloSelected(object objeto)
        {
            if (!_isNavigating)
            {
                _isNavigating = true;
                Syncfusion.ListView.XForms.ItemTappedEventArgs lista = objeto as Syncfusion.ListView.XForms.ItemTappedEventArgs;

                if (!(lista.ItemData is NameDTO modulo))
                    return;

                var parameters = new NavigationParameters
                    {
                        { "ModuloId", modulo.Id }
                    };
                await NavigationService.NavigateAsync(nameof(ComponentesModuloPage), parameters);
                _isNavigating = false;
            }
        }
        private void LoadModulos()
        {
            if (_apiService.CheckConnection())
            {
                ConsumoService();
            }
            else
            {
                ConsumoServiceLocal();
            }
        }



        private async void ConsumoServiceLocal()
        {
            LestomaLog.Normal("Consultando modulos.. offline");
            try
            {
                IsBusy = true;
                Modulos = new ObservableCollection<NameDTO>();
                var listado = await _unitOfWork.Componentes.GetModulos();

                if (listado.Any())
                {
                    Modulos = new ObservableCollection<NameDTO>(listado);
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


        private async void ConsumoService()
        {
            try
            {
                IsBusy = true;
                Modulos = new ObservableCollection<NameDTO>();
                ResponseDTO response = await _apiService.GetListAsyncWithToken<List<NameDTO>>(URL_API,
                    $"laboratorio-lestoma/listar-modulos-upa-actividad-por-usuario", TokenUser.Token);
                if (response.IsExito)
                {
                    var listado = (List<NameDTO>)response.Data;
                    if (listado.Count > 0)
                    {
                        Modulos = new ObservableCollection<NameDTO>(listado);
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

