using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Validaciones
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString())) {
                return ValidationResult.Success;
            }
            // le ponemos ! para indicar que no es nulo
            var primeraLetra = value.ToString()![0].ToString();

            if (primeraLetra != primeraLetra.ToUpper()) 
            {
                return new ValidationResult("La primer letra debe ser mayuscula");
            }

            return ValidationResult.Success;
        }
    }
}
