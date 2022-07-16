using DGScope.Receivers.Beast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DGScope.Receivers;
using DGScope.Library;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DGScope.AdsbUploadClient
{
    
    class AdsbUploader
    {
        AdsbUploadClientSettings settings = new AdsbUploadClientSettings();
        List<TrackUpdate> updates = new List<TrackUpdate>();
        public AdsbUploader()
        {
            if (File.Exists("uploadersettings.json"))
            {
                settings = AdsbUploadClientSettings.DeserializeFromJson(File.ReadAllText("uploadersettings.json"));
            }
        }

        public void Run()
        {
            settings.Receivers.Where(receiver => receiver.Enabled).ToList().ForEach(receiver =>
            {
                receiver.Start();
                receiver.AdsbUpdateReceived += Receiver_AdsbUpdateReceived;
            });
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                SendUpdates(FetchUpdates());
            }
        }

        private void Receiver_AdsbUpdateReceived(object sender, UpdateEventArgs e)
        {
            lock (updates)
            {
                updates.Add(e.Update as TrackUpdate);
            }
        }

        private TrackUpdate[] FetchUpdates()
        {
            List<TrackUpdate> sending = new List<TrackUpdate>();
            lock (updates)
            {
                updates.ForEach(update => sending.Add(update));
                sending.ForEach(update => updates.Remove(update));
            }
            return sending.ToArray();
        }
        private void SendUpdates(TrackUpdate[] sending)
        {
            if (sending.Length > 250)
            {
                TrackUpdate[] buffer;
                int i;
                for (i = 0; i + 250 < sending.Length; i+= 250)
                {
                    buffer = new TrackUpdate[250];
                    Array.Copy(sending, i, buffer, 0, 250);
                    SendUpdates(buffer);
                }
                buffer = new TrackUpdate[sending.Length - i];
                Array.Copy(sending, i, buffer, 0, sending.Length - i);
                sending = buffer;
            }
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                try
                {
                    string response = client.UploadString(settings.UploadUrl, JsonConvert.SerializeObject(sending));
                    Debug.WriteLine(response);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
