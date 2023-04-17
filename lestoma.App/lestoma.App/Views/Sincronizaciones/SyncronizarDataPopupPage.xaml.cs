using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;

namespace lestoma.App.Views.Sincronizaciones
{
    public partial class SyncronizarDataPopupPage : PopupPage
    {
        public SyncronizarDataPopupPage()
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
