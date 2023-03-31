using lestoma.CommonUtils.Helpers;
using Prism.Navigation;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class MACBluetoothPopupViewModel : BaseViewModel
    {
        private string _MAC;

        public MACBluetoothPopupViewModel(INavigationService navigationService) : base(navigationService)
        {
            _MAC = !string.IsNullOrWhiteSpace(MovilSettings.MacBluetooth) ? MovilSettings.MacBluetooth : string.Empty;
            SaveCommand = new Command(SaveClicked);
        }

        public Command SaveCommand { get; set; }

        public string MAC
        {
            get => _MAC;
            set => SetProperty(ref _MAC, value);
        }
        private async void SaveClicked(object obj)
        {
            if (string.IsNullOrWhiteSpace(_MAC) && _MAC.Length < 17)
            {
                AlertWarning("Debe digitar una MAC correcta.");
                return;
            }
            MovilSettings.MacBluetooth = _MAC;
            AlertSuccess("Se guardo la MAC satisfactoriamente.");
            await _navigationService.GoBackAsync();
        }
    }
}
