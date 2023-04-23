using lestoma.App.Views.Upas;
using lestoma.CommonUtils.Requests;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace lestoma.App.ViewModels
{
    public class InfoProtocolPopupViewModel : BaseViewModel
    {
        private ObservableCollection<ProtocoloRequest> _protocolos;
        public InfoProtocolPopupViewModel(INavigationService navigationService) : base(navigationService)
        {
            EditProtocolCommand = new Command<object>(EditProtocolClicked, CanNavigate);
        }
        public Command EditProtocolCommand { get; set; }
        public ObservableCollection<ProtocoloRequest> Protocolos
        {
            get => _protocolos;
            set => SetProperty(ref _protocolos, value);
        }
        private bool CanNavigate(object arg)
        {
            return true;
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
        private async void EditProtocolClicked(object obj)
        {
            try
            {
                var list = obj as Syncfusion.ListView.XForms.ItemTappedEventArgs;
                ProtocoloRequest protocolo = list.ItemData as ProtocoloRequest;

                if (protocolo == null)
                    return;
                var parameters = new NavigationParameters { { "dataProtocolo", protocolo } };
                await NavigationService.NavigateAsync($"{nameof(CreateEditProtocolPopupPage)}", parameters);
            }
            catch (System.Exception ex)
            {
                SeeError(ex);
            }
        }
    }
}
