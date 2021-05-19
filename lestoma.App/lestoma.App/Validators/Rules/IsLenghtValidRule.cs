using Xamarin.Forms.Internals;

namespace lestoma.App.Validators.Rules
{
    /// <summary>
    /// Validation rule for check the email has empty or null.
    /// </summary>
    /// <typeparam name="T">Not null or empty rule parameter</typeparam>
    [Preserve(AllMembers = true)]
    public class IsLenghtValidRule<T> : IValidationRule<T>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the validation Message.
        /// </summary>
        public string ValidationMessage { get; set; }
        public int MinimumLenght { get; set; }
        public int MaximumLenght { get; set; }


        #endregion

        #region Methods

        public bool Check(T value)
        {
            if (value == null)
            {
                return false;
            }
            var str = value as string;
            return (str.Length > MinimumLenght && str.Length <= MaximumLenght);
        }
    }

    #endregion
}
