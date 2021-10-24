using lestoma.App.Validators;
using lestoma.App.Validators.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace lestoma.App.Models
{
    public class UpaModel
    {
        public ValidatableObject<string> Nombre { get; set; } = new ValidatableObject<string>();
        public ValidatableObject<string> Descripcion { get; set; } = new ValidatableObject<string>();
        public ValidatableObject<string> CantidadActividades { get; set; } = new ValidatableObject<string>();
        public bool AreFieldsValid()
        {
            bool isNombreValid = Nombre.Validate();
            bool isDescripcionValid = Descripcion.Validate();
            bool isCantidadValid = CantidadActividades.Validate();

            return isNombreValid && isDescripcionValid && isCantidadValid;
        }
        public void AddValidationRules()
        {
            Nombre.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Nombre requerido." });
            Nombre.Validations.Add(new IsLenghtValidRule<string> {MaximumLenght = 30, MinimumLenght = 4, ValidationMessage = $"Debe tener entre 4 y 30 caracteres." });
            Descripcion.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "descripción requerida." });
            Descripcion.Validations.Add(new IsLenghtValidRule<string> { MaximumLenght = 500, MinimumLenght = 10, ValidationMessage = $"Debe tener entre 10 y 500 caracteres." });
            CantidadActividades.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "cantidad requerida." });
        }
    }
}
