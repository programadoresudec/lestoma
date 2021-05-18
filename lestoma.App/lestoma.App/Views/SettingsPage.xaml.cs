using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace lestoma.App.Views
{
    /// <summary>
    /// Page to show the setting.
    /// </summary>
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage" /> class.
        /// </summary>
        public SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}