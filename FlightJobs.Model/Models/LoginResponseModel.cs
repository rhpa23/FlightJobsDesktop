namespace FlightJobs.Model.Models
{
    public class LoginResponseModel
    {
        public string ActiveJobInfo { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
