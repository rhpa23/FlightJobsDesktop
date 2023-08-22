using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class SelectHostViewModel : ObservableObject
    {
        public string Option1HostUrl { get { return "https://flightjobs.bsite.net/"; } }
        public string Option2HostUrl { get { return "https://flightjobs.somee.com/"; } }
        public string Option3HostUrl { get { return "https://flightjobs2027-001-site1.htempurl.com/"; } }
        public string Option4HostUrl { get { return "http://localhost:5646/"; } }

        public bool Option1IsSelected { get; set; }
        public bool Option2IsSelected { get; set; }
        public bool Option3IsSelected { get; set; }
        public bool Option4IsSelected { get; set; }

        public bool Option1IsOnline { get; set; }
        public bool Option2IsOnline { get; set; }
        public bool Option3IsOnline { get; set; }
        public bool Option4IsOnline { get; set; }

        public string Option1Icon { get { if (Option1IsOnline) return "\uE704"; else return "\uF140"; } }
        public string Option2Icon { get { if (Option2IsOnline) return "\uE704"; else return "\uF140"; } }
        public string Option3Icon { get { if (Option3IsOnline) return "\uE704"; else return "\uF140"; } }
        public string Option4Icon { get { if (Option4IsOnline) return "\uE704"; else return "\uF140"; } }

        public string Option1IconColor { get { if (Option1IsOnline) return "Green"; else return "Red"; } }
        public string Option2IconColor { get { if (Option2IsOnline) return "Green"; else return "Red"; } }
        public string Option3IconColor { get { if (Option3IsOnline) return "Green"; else return "Red"; } }
        public string Option4IconColor { get { if (Option4IsOnline) return "Green"; else return "Red"; } }
    }
}
