using Xamarin.Forms.Internals;

namespace lestoma.App.Validators.Rules
{
    /// <summary>
    /// Validation rule for check the email has empty or null.
    /// </summary>
    /// <typeparam name="T">Not null or empty rule parameter</typeparam>
    [Preserve(AllMembers = true)]
    public class SizeOfString<T> : IValidationRule<T>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the validation Message.
        /// </summary>
        public string ValidationMessage { get; set; }



        #endregion

        #region Methods

        public bool Check(T value)
        {
            if (value == null)
            {
                return false;
            }
            var str = $"{value }";
            bool validarLength = str.Length >= 8 && str.Length <= 30 ? true : false;
            return validarLength;
        }
    }

    #endregion
}
