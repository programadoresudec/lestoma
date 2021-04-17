using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace lestoma.App.Views.Templates
{
    /// <summary>
    /// Social icon template.
    /// </summary>
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SocialIconTemplate : Grid
    {
        public SocialIconTemplate()
        {
            this.InitializeComponent();
        }
    }
}