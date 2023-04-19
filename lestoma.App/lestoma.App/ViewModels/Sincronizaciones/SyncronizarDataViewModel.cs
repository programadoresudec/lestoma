using Acr.UserDialogs;
using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.DTOs.Sync;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.DatabaseOffline.IConfiguration;
using Newtonsoft.Json;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Sincronizaciones
{
    public class SyncronizarDataViewModel : BaseViewModel
    {

        private readonly IApiService _apiService;
        private TipoSincronizacion _tipoSincronizacion;
        private IUnitOfWork _unitOfWork;
        private bool _isVisible;
        public SyncronizarDataViewModel(INavigationService navigationService, IApiService apiService, IUnitOfWork unitOfWork)
             : base(navigationService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            SyncronizationCommand = new Command(SyncDataClicked);
            CancelSyncronizationCommand = new Command(CancelSyncToMobileClicked);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("TypeSyncronization"))
            {
                TypeSync = parameters.GetValue<TipoSincronizacion>("TypeSyncronization");
                Debug.WriteLine($"tipo sincronización {TypeSync}");
                MessageHelp = EnumConfig.GetDescription(TypeSync);
                IsVisible = TypeSync == TipoSincronizacion.MigrateDataOnlineToDevice;
            }
        }

        public TipoSincronizacion TypeSync
        {
            get => _tipoSincronizacion;
            set => SetProperty(ref _tipoSincronizacion, value);
        }
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public Command SyncronizationCommand { get; }

        public Command CancelSyncronizationCommand { get; }
        private void SyncDataClicked(object obj)
        {
            switch (TypeSync)
            {
                case TipoSincronizacion.MigrateDataOnlineToDevice:
                    MigrateDataOnlineToDevice();
                    break;
                case TipoSincronizacion.MigrateDataOfflineToServer:
                    MigrateDataOfflineToServer();
                    break;
                default:
                    break;
            }
        }

        private async void MigrateDataOfflineToServer()
        {
            var count = await _unitOfWork.Laboratorio.ExistData();
            if (!count)
            {
                AlertWarning("No existe datos dentro del dispositivo móvil para migrar al servidor en la nube.");
                return;
            }
            var check = await UserDialogs.Instance.ConfirmAsync("¿Está seguro de sincronizar los datos al servidor en la nube?",
                     "Alerta", "Aceptar", "Cancelar");
            if (check)
            {

            }
        }

        private async void MigrateDataOnlineToDevice()
        {
            if (DependencyService.Resolve<IForegroundService>().IsForeGroundServiceRunning())
            {
                AlertWarning("Se está sincronizando los datos al dispositivo móvil, espere un momento...");
            }
            else
            {
                var check = await UserDialogs.Instance.ConfirmAsync("¿Está seguro de sincronizar los datos al dispositivo móvil?", "Alerta", "Aceptar", "Cancelar");

                if (check)
                {
                    if (!_apiService.CheckConnection())
                    {
                        AlertNoInternetConnection();
                        return;
                    }
                    DependencyService.Resolve<IForegroundService>().StartMyForegroundService();
                    _ = Task.Run(async () =>
                    {
                        var response = new ResponseDTO();
                        try
                        {
                            response = await _apiService.GetListAsyncWithToken<List<DataOnlineSyncDTO>>(URL_API,
                            "sincronizaciones-lestoma/sync-data-online-to-database-device", TokenUser.Token);
                            if (!response.IsExito)
                            {
                                LestomaLog.Error(response.MensajeHttp);
                                return;
                            }
                            var data = (List<DataOnlineSyncDTO>)response.Data;
                            LestomaLog.Normal("La data es:");
                            LestomaLog.Normal(JsonConvert.SerializeObject(data));

                            if (await _unitOfWork.Componentes.ExistData())
                            {
                                await _unitOfWork.Componentes.DeleteBulk();
                            }

                            var responseOffline = await _unitOfWork.Componentes.MigrateDataToDevice(data);
                            if (response.IsExito)
                            {
                                LestomaLog.Normal("Se ha migrado todos los datos.");
                            }
                        }
                        catch (Exception ex)
                        {
                            LestomaLog.Error(ex.Message);
                        }
                        finally
                        {
                            await Task.Delay(TimeSpan.FromSeconds(15));
                            if (response.IsExito)
                            {
                                AlertSuccess("Se migró satisfactoriamente los datos al dispositivo móvil.", 5);
                                MovilSettings.IsOnSyncToDevice = true;
                            }
                            else
                            {
                                MovilSettings.IsOnSyncToDevice = false;
                                AlertError("Ha fallado la migración al dispositivo móvil.", 5);
                            }
                            DependencyService.Resolve<IForegroundService>().StopMyForegroundService();
                        }
                    });
                    await _navigationService.GoBackAsync();
                    AlertSuccess("Se esta migrando los datos al dispositivo móvil.", 4);
                }
            }

        }
        private async void CancelSyncToMobileClicked(object obj)
        {
            try
            {
                if (!DependencyService.Resolve<IForegroundService>().IsForeGroundServiceRunning())
                {
                    AlertWarning("El servicio ya no se esta ejecutando y ha terminado.");
                }
                else
                {
                    var check = await UserDialogs.Instance.ConfirmAsync("¿Está seguro de cancelar la sincronización de los datos al dispositivo móvil?",
                    "Alerta", "Aceptar", "Cancelar");

                    if (check)
                    {
                        UserDialogs.Instance.ShowLoading("Cancelando...");
                        await Task.Delay(1000);
                        DependencyService.Resolve<IForegroundService>().StopMyForegroundService();
                        await _navigationService.GoBackAsync();
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
