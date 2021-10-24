using lestoma.App.Validators;
using lestoma.App.Validators.Rules;

namespace lestoma.App.Models
{
    public class ParametrosModel
    {
        public ValidatableObject<string> Nombre { get; set; } = new ValidatableObject<string>();
        public bool AreFieldsValid()
        {
            bool isNombreValid = Nombre.Validate();
            return isNombreValid;
        }
        public void AddValidationRules()
        {
            Nombre.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Nombre requerido." });
        }
    }
}
