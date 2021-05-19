using Xamarin.Forms.Internals;

namespace lestoma.App.Validators.Rules
{
    /// <summary>
    /// Validation Rule to check the email is valid or not.
    /// </summary>
    /// <typeparam name="T">Valid email rule parameter</typeparam>
    [Preserve(AllMembers = true)]
    class MatchPairValidationRule<T> : IValidationRule<ValidatablePair<T>>
    {
        #region Properties
        public string ValidationMessage { get; set; }
        #endregion

        #region Method
        public bool Check(ValidatablePair<T> value)
        {
            return value.Item1.Value.Equals(value.Item2.Value);
        }
        #endregion
    }
}
