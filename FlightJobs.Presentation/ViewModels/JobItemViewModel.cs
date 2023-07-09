using System.ComponentModel;

namespace FlightJobsDesktop.ViewModels
{
    public class JobItemViewModel
    {
        public bool IsSelected { get; set; }
        public int Id { get; set; }
        public string PayloadView { get; set; }
        public string PayloadLabel { get; set; }
        public long Pax { get; set; }
        public long Cargo { get; set; }
        public long Pay { get; set; }
        public string PayDisplayFormat { get { return string.Format("F{0:C0}", Pay); } }
        public bool FirstClass { get; set; }
        public bool IsCargo { get; set; }
        public string AviationType { get; set; }
        public long PilotScore { get; set; }
        public string PayloadIcon
        {
            get
            {
                if (IsCargo)
                    return "\uEB4E";
                else return "\uE716";
            }
        }
    }
}
