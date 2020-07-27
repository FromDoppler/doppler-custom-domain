using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DopplerCustomDomain
{
    // TODO: move validation inside constructor and ModelState updates inside a modelbinder,
    // see https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding?view=aspnetcore-3.1#custom-model-binder-sample
    // TODO: implement IEQualable and IComparable, add JsonConverter and TypeConverter attributes
    // see https://andrewlock.net/using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-2/
    public class CuitNumber : IValidatableObject
    {
        // TODO: rename as OriginalValue
        // Ugly patch, this value should come from de validated parameter, ie: /taxinfo/by-cuit/{cuit}
        // it wil be fixed after moving to a custom model binder
        public string? cuit { get; set; }
        public string SimplifiedValue => cuit?.Replace("-", "") ?? string.Empty;
        // TODO: add a new field Formatted Value, and return that value in ToString

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // By the moment it is also dealing with `required` validation
            if (string.IsNullOrWhiteSpace(SimplifiedValue))
            {
                return new[] { new ValidationResult("The CUIT number cannot be empty.", new[] { nameof(cuit) }) };
            }

            if (!SimplifiedValue.All(char.IsNumber))
            {
                return new[] { new ValidationResult("The CUIT number cannot have other characters than numbers and dashes.", new[] { nameof(cuit) }) };
            }

            if (SimplifiedValue.Length != 11)
            {
                return new[] { new ValidationResult("The CUIT number must have 11 digits.", new[] { nameof(cuit) }) };
            }

            if (!IsVerificationDigitValid(SimplifiedValue))
            {
                return new[] { new ValidationResult("The CUIT's verification digit is wrong.", new[] { nameof(cuit) }) };
            }

            return new ValidationResult[0];
        }


        // Source: https://es.wikipedia.org/wiki/Clave_%C3%9Anica_de_Identificaci%C3%B3n_Tributaria
        private static bool IsVerificationDigitValid(string normalizedCuit)
        {
            var factors = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

            var accumulated = 0;

            for (int i = 0; i < factors.Length; i++)
            {
                accumulated += int.Parse(normalizedCuit[i].ToString()) * factors[i];
            }

            accumulated = 11 - (accumulated % 11);

            if (accumulated == 11)
            {
                accumulated = 0;
            }

            if (int.Parse(normalizedCuit[10].ToString()) != accumulated)
            {
                return false;
            }

            return true;
        }
    }
}
