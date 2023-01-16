using System.Globalization;
using System.Windows.Controls;

namespace FlightJobsDesktop.ValidationRules
{
    public class MinimumCharacterRule : ValidationRule
    {
        public int MinimumCharacters { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string charString = value as string;

            if (charString?.Length >= MinimumCharacters)
                return new ValidationResult(true, null);

            return new ValidationResult(false, $"User atleast {MinimumCharacters} characters.");
        }
    }
}
