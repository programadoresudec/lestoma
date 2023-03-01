using lestoma.App.Models;
using Prism.Navigation;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace lestoma.App.ViewModels.Upas
{
    public class CreateProtocolPopupViewModel : BaseViewModel
    {
        private ProtocoloModel _protocolo;
        public CreateProtocolPopupViewModel(INavigationService navigationService) : base(navigationService)
        {
            _protocolo = new ProtocoloModel();
            SaveCommand = new Command(SaveClicked);
            Bytes = LoadBytes();
        }

        private List<int> LoadBytes()
        {
            return Enumerable.Range(0, 256).ToList();
        }

        public Command SaveCommand { get; set; }
        public List<int> Bytes { get; set; }

        public ProtocoloModel Protocolo
        {
            get => _protocolo;
            set => SetProperty(ref _protocolo, value);
        }
        private async void SaveClicked(object obj)
        {
            var parameters = new NavigationParameters
            {
                { "protocolo", _protocolo }
            };
            await _navigationService.GoBackAsync(parameters);
        }

    }
}
