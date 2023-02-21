using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;

namespace lestoma.App.Views.Reportes
{
    public partial class FilterDatePopupPage : PopupPage
    {
        public FilterDatePopupPage()
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
