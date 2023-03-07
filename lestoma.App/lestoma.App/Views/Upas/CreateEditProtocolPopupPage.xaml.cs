using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;

namespace lestoma.App.Views.Upas
{
    public partial class CreateEditProtocolPopupPage : PopupPage
    {
        public CreateEditProtocolPopupPage()
        {
            InitializeComponent();
        }
        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            CloseAllPopup();
        }
        private async void CloseAllPopup()
        {
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
