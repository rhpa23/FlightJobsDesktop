using System.Collections.Generic;

namespace FlightJobsDesktop.ViewModels
{
    public class AspnetUserViewModel : ObservableObject
    {
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> InfoCollection { get; private set; } = new Dictionary<string, string>();

        public string NickName { get; set; }
        private string _passwordConfirmed;
        public string PasswordConfirmed 
        {
            get 
            {
                return _passwordConfirmed;
            }
            set
            { 
                _passwordConfirmed = value;
                OnPropertyChanged(ref _passwordConfirmed, value);

                string result = "ok";

                if (!IsPasswordConfirmed)
                    result = "Passwords do NOT match";

                if (ErrorCollection.ContainsKey("PasswordConfirmedMsg"))
                    ErrorCollection["PasswordConfirmedMsg"] = result;
                else if (result != null)
                    ErrorCollection.Add("PasswordConfirmedMsg", result);

                OnPropertyChanged("ErrorCollection");
            }
        }
        private string _password;
        public string Password 
        {
            get 
            {
                return _password;
            }
            set
            {
                _password = value;

                var result = PasswordStrength;

                if (InfoCollection.ContainsKey("PasswordStrengthMsg"))
                    InfoCollection["PasswordStrengthMsg"] = result;
                else if (result != null)
                    InfoCollection.Add("PasswordStrengthMsg", result);

                OnPropertyChanged("InfoCollection");
            }
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged(ref _email, value);
            }
        }

        public bool IsPasswordConfirmed
        {
            get
            {
                return (_password != null && _passwordConfirmed != null && _password == _passwordConfirmed);
            }
            
        }
        private string PasswordStrength
        {
            get
            {
                int numberOfDigits = 0, numberOfLetters = 0, numberOfSymbols = 0;
                foreach (char c in _password)
                {
                    if (char.IsDigit(c))
                    {
                        numberOfDigits++;
                    }
                    else if (char.IsLetter(c))
                    {
                        numberOfLetters++;
                    }
                    else if (char.IsSymbol(c))
                    {
                        numberOfSymbols++;
                    }
                }

                if (numberOfDigits > 0 && numberOfSymbols > 0)
                {
                    return "Very strong";
                }
                else if (numberOfDigits.Equals(0) && numberOfSymbols.Equals(0))
                {
                    return "Weak";
                }
                else
                {
                    return "Strong";
                }
            }
        }
    }
}
