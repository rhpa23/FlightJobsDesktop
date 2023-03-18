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

            if (IsValid(charString, MinimumCharacters))
                return new ValidationResult(true, null);

            return new ValidationResult(false, $"At least {MinimumCharacters} characters.");
        }

        public static bool IsValid(string charString, int minimumCharacters)
        {
            return charString?.Length >= minimumCharacters;
        }
    }
}
