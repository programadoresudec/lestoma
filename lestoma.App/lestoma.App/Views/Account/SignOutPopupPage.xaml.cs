using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace lestoma.App.Views.Account
{
    public partial class SignOutPopupPage : PopupPage
    {
        public SignOutPopupPage()
        {
            InitializeComponent();
            AnnounceBindingContext();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            AnnounceBindingContext();
        }

        private void AnnounceBindingContext()
        {
            System.Diagnostics.Debug.WriteLine(GetType().Name);
            System.Diagnostics.Debug.WriteLine($"BindingContext: {BindingContext?.GetType()?.Name}");
        }
    }
}
