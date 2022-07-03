using DGScope.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Mail;
using Newtonsoft.Json;

namespace ScopeServer
{
    public class PatWatch
    {
        private Timer timer;

        public int ModeSAddress { get; set; }
        public string Email { get; set; }
        public PatWatch(int modeSAddress, string email)
        {
            ModeSAddress = modeSAddress;
            Email = email;
            timer = new Timer(60000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }
        public PatWatch() 
        {
            timer = new Timer(60000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }
        private List<FlightPlan> pats = new List<FlightPlan>();
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.Facilities.ToList().ForEach(facility => facility.FlightPlans.ToList().Where(fp =>  fp.AssociatedTrack != null && fp.AssociatedTrack.ModeSCode == ModeSAddress).ToList()
            .ForEach(pat => { if (!pats.Contains(pat)) { pats.Add(pat); pat.Updated += Pat_Updated; sendEmail(pat, facility.FacilityID, Email); } }));
        }
        private void sendEmail(object obj, string facility, string emailAddress)
        {
            MailMessage message = new MailMessage(Settings.EmailSettings.FromAddress, emailAddress);
            message.Subject = string.Format("Facility {0} updated flight plan information for a watched address", facility);
            message.Body = JsonConvert.SerializeObject(obj);
            try
            {
                Settings.SmtpClient.Send(message);
                Console.WriteLine("Sent email.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void Pat_Updated(object sender, UpdateEventArgs e)
        {
            string facilityid = string.Empty;
            lock (Settings.Facilities)
                facilityid = Settings.Facilities.Where(facility => facility.FlightPlans.Contains(e.Update.Base)).FirstOrDefault().FacilityID;
            sendEmail(e.Update, facilityid, Email);
        }
    }
}
