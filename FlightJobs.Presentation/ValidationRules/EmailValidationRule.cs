using System.Globalization;
using System.Windows.Controls;

namespace FlightJobsDesktop.ValidationRules
{
    public class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (IsValidEmail(value?.ToString())) return ValidationResult.ValidResult;

            return new ValidationResult(false, "Email is invalid");
        }

        bool IsValidEmail(string email)
        {
            var trimmedEmail = email?.Trim();

            if (trimmedEmail == null || trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
