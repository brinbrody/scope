using DGScope.Library;
using DGScope.Receivers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScopeServer
{
    public class Settings
    {
        private static ObservableCollection<Facility> facilities;
        private static List<PatWatch> patWatches;
        private static ReceiverList receivers;
        private static bool started;
        private static System.Timers.Timer garbageCollectionTimer;
        private static EmailSettings emailSettings;
        private static SmtpClient smtpClient;

        private static int garbageCollectionInterval = 300; //5 minutes


        public static ObservableCollection<Facility> Facilities
        {
            get
            {
                if (facilities == null)
                    facilities = new ObservableCollection<Facility>();
                return facilities;
            }
        }
        public static EmailSettings EmailSettings
        {
            get
            {
                if (emailSettings == null)
                    emailSettings = new EmailSettings();
                return emailSettings;
            }
        }

        public static List<PatWatch> PatWatches
        {
            get
            {
                if (patWatches == null)
                    patWatches = new List<PatWatch>();
                return patWatches;
            }
        }

        public static SmtpClient SmtpClient
        {
            get
            {
                if (smtpClient == null)
                {
                    smtpClient = new SmtpClient();
                    if (emailSettings != null)
                    {
                        smtpClient.Host = emailSettings.Host;
                        smtpClient.Port = emailSettings.Port;
                        if (!string.IsNullOrEmpty(emailSettings.Username) && !string.IsNullOrEmpty(emailSettings.Username))
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);
                        }
                        else
                        {
                            smtpClient.UseDefaultCredentials = true;
                        }
                    }
                }
                return smtpClient;
            }
        }
        public static void StartReceivers()
        {
            if (started)
                return;
            started = true;
            foreach (var item in Receivers)
            {
                item.SetFacilityList(Facilities);
                item.Start();
            }
            garbageCollectionTimer = new System.Timers.Timer(garbageCollectionInterval * 1000);
            garbageCollectionTimer.Start();
            garbageCollectionTimer.Elapsed += GarbageCollectionTimer_Elapsed;
        }

        private static void GarbageCollectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (Facilities)
                foreach (Facility facility in Facilities)
                {
                    lock (facility)
                    {
                        lock (facility.Tracks)
                            facility.Tracks.Where(track => track.LastMessageTime < DateTime.UtcNow.AddSeconds(-garbageCollectionInterval)).ToList().ForEach(  x => {
                                facility.Tracks.Remove(x);
                                x.InvokeDeleted();
                            });
                        lock (facility.FlightPlans)
                            facility.FlightPlans.Where(flightPlan => flightPlan.LastMessageTime < DateTime.UtcNow.AddSeconds(-garbageCollectionInterval)).ToList().ForEach(x => {
                                facility.FlightPlans.Remove(x);
                                x.InvokeDeleted();
                            });
                    }
                }
        }

        public static void LoadFacilities()
        {
            foreach (var file in Directory.GetFiles(".", "*.adaptjson"))
            {
                var newFacility = new Facility();
                Facilities.Add(newFacility);
                newFacility.Adaptation = Adaptation.DeserializeFromJsonFile(file);
            }
        }
        public static ReceiverList Receivers
        {
            get
            {
                if (receivers == null)
                {
                    var json = File.ReadAllText("receivers.json");
                    receivers = ReceiverList.DeserializerFromJson(json);
                }
                if (receivers == null)
                {
                    receivers = new ReceiverList();
                }
                return receivers;
            }
        }
    }
}
