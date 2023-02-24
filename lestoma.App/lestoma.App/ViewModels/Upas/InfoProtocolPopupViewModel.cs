using lestoma.CommonUtils.DTOs;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace lestoma.App.ViewModels
{
    public class InfoProtocolPopupViewModel : BaseViewModel
    {
        private ObservableCollection<ProtocoloDTO> _protocolos;
        public InfoProtocolPopupViewModel(INavigationService navigationService) :
            base(navigationService)
        {

        }
        public ObservableCollection<ProtocoloDTO> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("protocolos"))
            {
                var protocolos = parameters.GetValue<object>("protocolos");
                Protocolos = new ObservableCollection<ProtocoloDTO>((List<ProtocoloDTO>)protocolos);
            }
        }
    }
}
