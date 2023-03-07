using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace lestoma.App.ViewModels
{
    public class InfoProtocolPopupViewModel : BaseViewModel
    {
        private ObservableCollection<ProtocoloRequest> _protocolos;
        public InfoProtocolPopupViewModel(INavigationService navigationService) :
            base(navigationService)
        {

        }
        public ObservableCollection<ProtocoloRequest> Protocolos
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
                Protocolos = new ObservableCollection<ProtocoloRequest>((List<ProtocoloRequest>)protocolos);
            }
        }
    }
}
