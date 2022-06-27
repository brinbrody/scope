using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using DGScope.Library;
using System.Collections.ObjectModel;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.IO;
using DGScope.Receivers;
using DGScope.Receivers.FAA_STDDS;
using Newtonsoft.Json;
using System.Linq;
using BAMCIS.GeoJSON;

namespace ScopeServer.Controllers
{
    public class UpdateController : Controller
    {
        List<Update> PendingUpdates = new List<Update>();
        Facility selectedFacility;
        string selectedFacilityID;
        
        [HttpGet]
        [Route("aircraft.geojson/{facilityID}")]
        public void GetAircraftGeoJson(string facilityID)
        {
            List<Feature> features = new List<Feature>();
            foreach (Facility facility in Settings.Facilities.Where(x=> x.FacilityID == facilityID))
            {
                foreach (FlightPlan plan in facility.FlightPlans.Where(p => p.AssociatedTrack != null))
                {
                    Track track = plan.AssociatedTrack;
                    Point location = new Point(new Position(track.Location.Longitude, track.Location.Latitude, track.Altitude.TrueAltitude * 0.3048 ));
                    Feature plane = new Feature(location);
                    plane.Properties.Add("callsign", plan.Callsign);
                    plane.Properties.Add("altitude", (track.Altitude.TrueAltitude / 100).ToString().PadLeft(3, '0'));
                    plane.Properties.Add("owner", plan.Owner);
                    features.Add(plane);
                }
            }
            GeoJson json = new FeatureCollection(features);
            using (var writer = new StreamWriter(this.Response.Body))
                writer.Write(json.ToJson());
        }
        [HttpGet]
        [Route("{facilityID}/updates")]
        public async Task GetUpdates(string facilityID)
        {
            this.Response.StatusCode = 200;
            this.Response.Headers.Add(HeaderNames.ContentType, "application/json");
            Settings.StartReceivers();
            
            SetFacilityID(facilityID);
            while (true)
            {
                List<Update> sending;
                lock (PendingUpdates)
                    sending = new List<Update>(PendingUpdates);
                if (sending.Count == 0)
                    System.Threading.Thread.Sleep(100);
                else
                    foreach (Update update in sending)
                    {
                        lock (PendingUpdates)
                            PendingUpdates.Remove(update);
                        if (update == null)
                            continue;
                        using (StreamWriter writer = new StreamWriter(this.Response.Body))
                        {
                            await writer.WriteLineAsync(update.SerializeToJsonAsync().Result);
                        }
                }
                
            }
        }
        [HttpGet]
        [Route("{facilityID}/adaptation")]
        public void GetAdaptation(string facilityID)
        {
            this.Response.StatusCode = 200;
            this.Response.Headers.Add(HeaderNames.ContentType, "application/json");
            SetFacilityID(facilityID);
            using (StreamWriter writer = new StreamWriter(this.Response.Body))
            {
                writer.Write(Adaptation.SerializeToJson(selectedFacility.Adaptation));
            }
        }
        [HttpPost]
        [Route("{facilityID}/update")]
        public void PostUpdate(string facilityID)
        {
            Response.StatusCode = 403;
        }
        private void SetFacilityID(string facilityID)
        {
            selectedFacility = Settings.Facilities.Where(x => x.FacilityID == facilityID).FirstOrDefault();
            if (selectedFacility != null)
                AddFacilityWatchers(selectedFacility);
            else
            {
                selectedFacilityID = facilityID;
                Settings.Facilities.CollectionChanged += Facilities_CollectionChanged;
            }

        }
        private void AddFacilityWatchers(Facility facility)
        {
            lock (facility.Tracks)
                foreach (Track track in facility.Tracks)
                {
                    lock(PendingUpdates)
                        PendingUpdates.Add(track.GetCompleteTrackUpdate());
                    track.Updated += UpdateReceived;
                }
            lock (facility.FlightPlans)
                foreach (FlightPlan flightPlan in facility.FlightPlans)
                {
                    lock (PendingUpdates)
                        PendingUpdates.Add(flightPlan.GetCompleteFlightPlanUpdate());
                    flightPlan.Updated += UpdateReceived;
                }
            facility.Tracks.CollectionChanged += CollectionChanged;
            facility.FlightPlans.CollectionChanged += CollectionChanged;
        }
        private void RemoveFacilityWatchers(Facility facility)
        {
            facility.Tracks.CollectionChanged -= CollectionChanged;
            facility.FlightPlans.CollectionChanged -= CollectionChanged;
        }
        private void Facilities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Facility item in e.NewItems)
                    {
                        if (item.FacilityID == selectedFacilityID)
                            AddFacilityWatchers(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(Facility item in e.OldItems)
                    {
                        RemoveFacilityWatchers(item);
                    }
                    break;
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IUpdatable item in e.NewItems)
                    {
                        item.Updated += UpdateReceived;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IUpdatable item in e.OldItems)
                    {
                        item.Updated -= UpdateReceived;
                    }
                    break;
            }
        }

        private void UpdateReceived(object sender, UpdateEventArgs e)
        {
            lock (PendingUpdates)
                PendingUpdates.Add(e.Update);
        }
    }

    public static class Settings
    {
        private static ObservableCollection<Facility> facilities;
        private static ReceiverList receivers;
        private static bool started;
        private static System.Timers.Timer garbageCollectionTimer;
        private static int garbageCollectionInterval = 30000; //30 seconds

        public static ObservableCollection<Facility> Facilities
        {
            get
            {
                if (facilities == null)
                    facilities = new ObservableCollection<Facility>();
                return facilities;
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
            garbageCollectionTimer = new System.Timers.Timer(garbageCollectionInterval);
            garbageCollectionTimer.Start();
            garbageCollectionTimer.Elapsed += GarbageCollectionTimer_Elapsed;
        }

        private static void GarbageCollectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock(Facilities)
                foreach (Facility facility in Facilities)
                {
                    lock (facility)
                    {
                        lock(facility.Tracks)
                            facility.Tracks.Where(track => track.LastMessageTime < DateTime.UtcNow.AddSeconds(-garbageCollectionInterval)).ToList().ForEach(x => facility.Tracks.Remove(x));
                        lock (facility.FlightPlans)
                            facility.FlightPlans.Where(flightPlan => flightPlan.LastMessageTime < DateTime.UtcNow.AddSeconds(-garbageCollectionInterval)).ToList().ForEach(x => facility.FlightPlans.Remove(x));
                    }
                }
        }

        public static void LoadFacilities()
        {
            foreach (var file in Directory.GetFiles(".","*.adaptjson"))
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
