using Xamarin.Forms.Internals;

namespace lestoma.App.Validators.Rules
{
    /// <summary>
    /// Validation rule for check the email has empty or null.
    /// </summary>
    /// <typeparam name="T">Not null or empty rule parameter</typeparam>
    [Preserve(AllMembers = true)]
    public class IsNotEquals<T,O> 
    {
        #region Properties

        /// <summary>
        /// Gets or sets the validation Message.
        /// </summary>
        public string ValidationMessage { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Check the Email has null or empty
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>returns bool value</returns>
        public bool Check(T value, O value2)
        {
            if (value == null && value2 == null)
            {
                return false;
            }

            var str = $"{value }";
            var str2 = $"{value2 }";
            return str.Equals(str2);
        }

        #endregion
    }
}
