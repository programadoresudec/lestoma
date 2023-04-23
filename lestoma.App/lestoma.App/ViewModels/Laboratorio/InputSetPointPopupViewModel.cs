using Acr.UserDialogs;
using lestoma.App.Views.Laboratorio;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Laboratorio
{
    public class InputSetPointPopupViewModel : BaseViewModel
    {
        private TramaComponenteRequest _componenteRequest;
        private float? _inputSetPoint;
        public InputSetPointPopupViewModel(INavigationService navigationService) :
               base(navigationService)
        {
            _componenteRequest = new TramaComponenteRequest();
            SaveCommand = new Command(SaveClicked);
        }

        public TramaComponenteRequest TramaComponente
        {
            get => _componenteRequest;
            set => SetProperty(ref _componenteRequest, value);
        }
        public float? InputSetPoint
        {
            get => _inputSetPoint;
            set => SetProperty(ref _inputSetPoint, value);
        }

        public Command SaveCommand { get; set; }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("tramaComponente"))
            {
                TramaComponente = parameters.GetValue<TramaComponenteRequest>("tramaComponente");
            }
        }
        private async void SaveClicked()
        {
            try
            {
                if (!InputSetPoint.HasValue || InputSetPoint.Value > 500)
                {
                    AlertWarning("Es requerido un valor de 0 a 500.");
                    return;
                }
                UserDialogs.Instance.ShowLoading("Guardando...");

                var request = new TramaComponenteSetPointRequest
                {
                    ComponenteId = TramaComponente.ComponenteId,
                    NombreComponente = TramaComponente.NombreComponente,
                };
                var bytesFlotante = Reutilizables.IEEEFloatingPointToByte(InputSetPoint.Value);
                TramaComponente.TramaOchoBytes[4] = bytesFlotante[0];
                TramaComponente.TramaOchoBytes[5] = bytesFlotante[1];
                TramaComponente.TramaOchoBytes[6] = bytesFlotante[2];
                TramaComponente.TramaOchoBytes[7] = bytesFlotante[3];
                request.ValorSetPoint = InputSetPoint.Value;
                request.TramaWithCRC = Reutilizables.TramaConCRC16Modbus(TramaComponente.TramaOchoBytes);
                var parameters = new NavigationParameters
                {
                    { "tramaComponente", request }
                };

                await NavigationService.NavigateAsync(nameof(SetPointPage), parameters);
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
