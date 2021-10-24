using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace lestoma.App.Behaviors
{
    class PickerLineValidationBehaviour : BehaviorBase<Picker>
    {
        #region Fields

        public static readonly BindableProperty IsValidProperty =
            BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(EntryLineValidationBehaviour), true, BindingMode.TwoWay, null);
        #endregion

        #region Properties     
        public bool IsValid
        {
            get
            {
                return (bool)this.GetValue(IsValidProperty);
            }

            set
            {
                this.SetValue(IsValidProperty, value);
            }
        }
        #endregion
        #region Methods  
        protected override void OnAttachedTo(BindableObject bindable)
        {
            base.OnAttachedTo(bindable);

            this.AssociatedObject.Focused += this.AssociatedObject_Focused;
        }

        protected override void OnDetachingFrom(BindableObject bindable)
        {
            base.OnDetachingFrom(bindable);
            this.AssociatedObject.Focused -= this.AssociatedObject_Focused;
        }

        private void AssociatedObject_Focused(object sender, FocusEventArgs e)
        {
            this.IsValid = true;
        }
    }
    #endregion
}
