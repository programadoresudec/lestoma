using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Interfaces;
using lestoma.DatabaseOffline.IConfiguration;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Sincronizaciones
{
    public class SyncronizarDataViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private TipoSincronizacion _tipoSincronizacion;
        public IForegroundService foregroundService;
        public SyncronizarDataViewModel(INavigationService navigationService, 
            IApiService apiService)
             : base(navigationService)
        {
            _apiService = apiService;
            SyncronizationCommand = new Command(SyncDataClicked);
        }


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("TypeSyncronization"))
            {
                TypeSync = parameters.GetValue<TipoSincronizacion>("TypeSyncronization");
                System.Diagnostics.Debug.WriteLine($"tipo sincronización {TypeSync}");
            }
        }

        public TipoSincronizacion TypeSync
        {
            get => _tipoSincronizacion;
            set => SetProperty(ref _tipoSincronizacion, value);
        }
        public Command SyncronizationCommand { get; }


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
            throw new NotImplementedException();
        }

        private async void MigrateDataOnlineToDevice()
        {
            //Task.Run(async () => {

            //});
            //Task.Run(async () => {

            //});
            //Task.Run(async () => {

            //});

        }
    }
}
