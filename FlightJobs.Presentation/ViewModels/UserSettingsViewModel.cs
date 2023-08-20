using FlightJobs.Connect.MSFS.SDK.Model;
using Newtonsoft.Json;

namespace FlightJobsDesktop.ViewModels
{
    public class UserSettingsViewModel : ObservableObject
    {
        [JsonIgnoreAttribute]
        public string Username { get; set; }

        [JsonIgnoreAttribute]
        public string Password { get; set; }

        [JsonIgnoreAttribute]
        public string WeightUnit { get; set; } = "kg";

        [JsonIgnoreAttribute]
        public bool ReceiveAlertEmails { get; set; }

        [JsonIgnoreAttribute]
        public bool IsWeightUnitKg
        {
            get { return WeightUnit.Contains("kg"); }
            set
            {
                if (value) WeightUnit = "kg";
                else WeightUnit = "Pounds";
            }
        }

        [JsonIgnoreAttribute]
        public bool IsWeightUnitPounds 
        { 
            get { return !WeightUnit.Contains("kg"); } 
            set 
            { 
                if (value) WeightUnit = "Pounds"; 
                else WeightUnit = "kg"; 
            } 
        }

        public bool StartInSysTray { get; set; }
        public bool ExitWithFS { get; set; }
        public bool AutoStartJob { get; set; }
        public bool AutoFinishJob { get; set; }
        public bool ShowLandingData { get; set; }
        public int SelectedHostOption { get; set; }

        public string ThemeName { get; set; }

        [JsonIgnoreAttribute]
        public bool IsDarkTheme { get { return ThemeName == "Dark"; } }

        public SimDataModel CurrentSimData { get; set; }

        private string _simbriefUsername { get; set; }
        public string SimbriefUsername
        {
            get { return _simbriefUsername; }
            set { _simbriefUsername = value; OnPropertyChanged(); }
        }
    }
}
