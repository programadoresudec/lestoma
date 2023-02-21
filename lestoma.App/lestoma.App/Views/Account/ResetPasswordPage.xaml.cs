using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using System.Diagnostics;

namespace lestoma.App.Views.Account
{
    /// <summary>
    /// Page to reset old password
    /// </summary>
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResetPasswordPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResetPasswordPage" /> class.
        /// </summary>
        public ResetPasswordPage()
        {
            this.InitializeComponent();
        }

        private void CodeOneEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var tabs = LayoutCode.GetTabIndexesOnParentPage(out int count);
                var visual = sender as VisualElement;
                var currentIndex = visual.TabIndex;
                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                    return;
                var nextFocus = LayoutCode.FindNextElement(true, tabs, ref currentIndex);
                (nextFocus as VisualElement)?.Focus();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}