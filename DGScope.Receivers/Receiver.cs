using DGScope.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DGScope.Receivers
{
    public abstract class Receiver
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        [JsonIgnore]
        public List<Facility> Facilities { get; private set; } = new List<Facility>();
        public abstract void Start();
        public abstract void Stop();
        public void Restart(int sleep = 0)
        {
            Stop();
            System.Threading.Thread.Sleep(sleep);
            Start();
        }

        public void SetFacilityList(List<Facility> facilities)
        {
            Facilities = facilities;
        }
        public Track GetTrack(int icaoID, string? facilityID = null)
        {
            Track track;
            List<Track> tracks;
            List<Facility> facilities;
            if (facilityID == null)
            {
                tracks = new List<Track>();
                Facilities.ForEach(x => tracks.AddRange(x.Tracks));
                facilities = Facilities;
            }
            else
            {
                var facility = Facilities.Where(x => x.FacilityID == facilityID).FirstOrDefault();
                if (facility == null)
                {
                    facility = new Facility() { FacilityID = facilityID };
                    Facilities.Add(facility);
                }
                tracks = facility.Tracks;
                facilities = new List<Facility>() { facility };
            }
            
            lock (tracks)
            {
                track = (from x in tracks where x.ModeSCode == icaoID select x).FirstOrDefault();
                if (track == null)
                {
                    track = new Track(icaoID);
                    facilities.ForEach(x => x.Tracks.Add(track));
                    Debug.WriteLine("Added airplane {0} from {1}", icaoID.ToString("X"), Name);
                }
            }

            return track;
        }

        public Track GetTrack(Guid guid)
        {
            Track track = null;
            List<Track> tracks = new List<Track>();
            Facilities.ForEach(x => tracks.AddRange(x.Tracks));
            track = tracks.Where(x => x.Guid == guid).FirstOrDefault();
            return track;
        }

        public FlightPlan GetFlightPlan(Guid guid)
        {
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans = new List<FlightPlan>();
            Facilities.ForEach(x => flightPlans.AddRange(x.FlightPlans));
            flightPlan = flightPlans.Where(x => x.Guid == guid).FirstOrDefault();
            return flightPlan;
        }

        public FlightPlan GetFlightPlan(string callsign, string facilityID)
        {
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans;
            Facility facility;
            if (facilityID == null)
                return null;
            facility = Facilities.Where(x => x.FacilityID == facilityID).FirstOrDefault();
            if (facility == null)
            {
                facility = new Facility() { FacilityID = facilityID };
                Facilities.Add(facility);
            }
            flightPlans = facility.FlightPlans;
            lock (flightPlans)
            {
                flightPlan = (from x in flightPlans where x.Callsign == callsign select x).FirstOrDefault();
                if (flightPlan == null)
                {
                    flightPlan = new FlightPlan(callsign);
                    facility.FlightPlans.Add(flightPlan);
                    Debug.WriteLine("Added flight plan {0} from {1}", callsign, Name);
                }
            }
            return flightPlan;
        }
    }
}
