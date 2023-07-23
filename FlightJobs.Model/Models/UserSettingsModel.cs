namespace FlightJobs.Model.Models
{
    public class UserSettingsModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string WeightUnit { get; set; }
        public string Password { get; set; }
        public bool ReceiveAlertEmails { get; set; }
        public UserSettingsLocalModel LocalSettings { get; set; } = new UserSettingsLocalModel();
    }

    public class UserSettingsLocalModel
    {
        public bool StartInSysTray { get; set; }
        public bool ExitWithFS { get; set; }
        public bool AutoStartJob { get; set; }
        public bool AutoFinishJob { get; set; }
        public bool ShowLandingData { get; set; }
        public string ThemeName { get; set; }
        public string SimConnectStatus { get; set; }
        public string SimbriefUsername { get; set; }
    }
}
