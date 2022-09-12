using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace lestoma.App.Controls
{
    /// <summary>
    /// This class is extended from Xamarin.Forms.Editor to extend the size and to remove the border for the editor control in the Android and UWP platforms.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class BorderlessEditor : Editor
    {
        public static BindableProperty PlaceholderProperty
            = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(BorderlessEditor));

        public static BindableProperty PlaceholderColorProperty
           = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(BorderlessEditor), Color.LightGray);

        public static BindableProperty HasRoundedCornerProperty
        = BindableProperty.Create(nameof(HasRoundedCorner), typeof(bool), typeof(BorderlessEditor), false);

        public static BindableProperty IsExpandableProperty
        = BindableProperty.Create(nameof(IsExpandable), typeof(bool), typeof(BorderlessEditor), false);

        public bool IsExpandable
        {
            get { return (bool)GetValue(IsExpandableProperty); }
            set { SetValue(IsExpandableProperty, value); }
        }
        public bool HasRoundedCorner
        {
            get { return (bool)GetValue(HasRoundedCornerProperty); }
            set { SetValue(HasRoundedCornerProperty, value); }
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        public BorderlessEditor()
        {
            TextChanged += OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsExpandable) InvalidateMeasure();
        }
    }
}