using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;

namespace lestoma.App.Views.Buzon
{
    public partial class MoreInfoPopupPage : PopupPage
    {
        public MoreInfoPopupPage()
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
