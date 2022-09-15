using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace lestoma.App.Views
{
    public partial class MessagePopupPage : PopupPage
    {
        public MessagePopupPage(string message, string icon = "icon_info.png")
        {
            InitializeComponent();
            Label_Message.Text = message;
            Image_Icon.Source = icon;
        }

        private async void Button_Salir_Clicked(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
