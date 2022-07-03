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
        DateTime stateSent;
        
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
            if (this.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var useragent))
                Console.WriteLine(useragent.First());
            else
                Console.WriteLine("Get");
            SetFacilityID(facilityID);
            while (!HttpContext.RequestAborted.IsCancellationRequested)
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
        [Route("{facilityID}/facilityState")]
        public void GetAdaptation(string facilityID)
        {
            this.Response.StatusCode = 200;
            this.Response.Headers.Add(HeaderNames.ContentType, "application/json");
            SetFacilityID(facilityID);
            using (StreamWriter writer = new StreamWriter(this.Response.Body))
            {
                writer.Write(JsonConvert.SerializeObject(selectedFacility));
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
            {
                lock (facility.FlightPlans)
                {
                    facility.Tracks.ToList().ForEach(x => PendingUpdates.Add(x.GetCompleteUpdate()));
                    facility.FlightPlans.ToList().ForEach(x => PendingUpdates.Add(x.GetCompleteUpdate()));
                    facility.Tracks.ToList().ForEach(x => x.Updated += UpdateReceived);
                    facility.FlightPlans.ToList().ForEach(x => x.Updated += UpdateReceived);
                    facility.Tracks.CollectionChanged += CollectionChanged;
                    facility.FlightPlans.CollectionChanged += CollectionChanged;
                }
            }
            stateSent = DateTime.UtcNow;
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
                        PendingUpdates.Add(item.GetCompleteUpdate());
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
            if (e.Update.TimeStamp < stateSent)
                return;
            lock (PendingUpdates)
                PendingUpdates.Add(e.Update);
        }
    }

    
}
