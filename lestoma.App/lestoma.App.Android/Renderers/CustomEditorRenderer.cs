using Android.Graphics.Drawables;
using lestoma.App.Controls;
using lestoma.App.Droid;
using System.ComponentModel;
using Xamarin.Forms;
using Application = Android.App.Application;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessEditor), typeof(CustomEditorRenderer))]
namespace lestoma.App.Droid
{
    public class CustomEditorRenderer : EditorRenderer
    {
        bool initial = true;
        Drawable originalBackground;

        public CustomEditorRenderer()
           : base(Application.Context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                if (initial)
                {
                    originalBackground = Control.Background;
                    initial = false;
                }

            }

            if (e.NewElement != null)
            {
                var customControl = (BorderlessEditor)Element;
                if (customControl.HasRoundedCorner)
                {
                    ApplyBorder();
                }

                if (!string.IsNullOrEmpty(customControl.Placeholder))
                {
                    Control.Hint = customControl.Placeholder;
                    Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());

                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var customControl = (BorderlessEditor)Element;

            if (BorderlessEditor.PlaceholderProperty.PropertyName == e.PropertyName)
            {
                Control.Hint = customControl.Placeholder;

            }
            else if (BorderlessEditor.PlaceholderColorProperty.PropertyName == e.PropertyName)
            {

                Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());

            }
            else if (BorderlessEditor.HasRoundedCornerProperty.PropertyName == e.PropertyName)
            {
                if (customControl.HasRoundedCorner)
                {
                    ApplyBorder();

                }
                else
                {
                    this.Control.Background = originalBackground;
                }
            }
        }

        void ApplyBorder()
        {
            GradientDrawable gd = new GradientDrawable();
            gd.SetCornerRadius(10);
            gd.SetStroke(2, Color.Black.ToAndroid());
            this.Control.Background = gd;
        }
    }
}