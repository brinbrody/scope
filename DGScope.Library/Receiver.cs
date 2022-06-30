using DGScope.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DGScope.Receivers
{
    public abstract class Receiver
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        [JsonIgnore]
        public ObservableCollection<Facility> Facilities { get; private set; }
        public abstract void Start();
        public abstract void Stop();
        public void Restart(int sleep = 0)
        {
            Stop();
            System.Threading.Thread.Sleep(sleep);
            Start();
        }

        public void SetFacilityList(ObservableCollection<Facility> facilities)
        {
            Facilities = facilities;
        }
        public Track GetTrack(int icaoID, string facilityID)
        {
            Track track;
            List<Track> tracks;
            Facility facility;
            if (facilityID == null)
                return null;
            facility = GetFacility(facilityID);
            lock (facility.Tracks)
            {
                tracks = facility.Tracks.ToList();
                track = (from x in tracks where x.ModeSCode == icaoID select x).FirstOrDefault();
                if (track == null)
                {
                    track = new Track(icaoID, facility);
                    facility.Tracks.Add(track);
                    track.Altitude.SetAltitudeProperties(facility.Adaptation.TransitionAltitude, facility.Altimeter);
                    Debug.WriteLine("Added airplane {0} from {1}", icaoID.ToString("X"), Name);
                }
            }

            return track;
        }

        public Track GetTrack(Guid guid, string facilityID = null)
        {
            List<Track> tracks = new List<Track>();
            Facilities.ToList().ForEach(facility => { lock (facility.Tracks) tracks.AddRange(facility.Tracks.Where(x => x.Guid == guid)); });
            Track track = tracks.FirstOrDefault();
            if (track == null && facilityID != null)
            {
                var facility = GetFacility(facilityID);
                track = new Track(guid, GetFacility(facilityID));
                lock (facility.Tracks)
                    facility.Tracks.Add(track);
            }
            return track;
        }
        public List<Track> GetTracks (int modeSCode, GeoPoint trackLocation)
        {
            List<Track> tracks = new List<Track>();
            if (trackLocation != null)
            {
                var facilities = Facilities.ToList().Where(facility => facility.Adaptation.FacilityCenter.DistanceTo(trackLocation) <= facility.Adaptation.MaxRange);
                foreach (Facility facility in facilities)
                {
                    tracks.Add(GetTrack(modeSCode, facility.FacilityID));
                }
            }
            return tracks;
        }
        public List<Track> GetTracks (int modeSCode, string facilityID = null)
        {
            return new List<Track>() { GetTrack(modeSCode, facilityID) };
        }
        public List<Track> GetTracks(Guid guid, string facilityID = null)
        {
            return new List<Track>() { GetTrack(guid, facilityID) };
        }
        public FlightPlan GetFlightPlan(Guid guid)
        {
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans = new List<FlightPlan>();
            Facilities.ToList().ForEach(x => { lock (flightPlans) flightPlans.AddRange(x.FlightPlans); });
            flightPlan = flightPlans.Where(x => x.Guid == guid).FirstOrDefault();
            return flightPlan;
        }
        public Facility GetFacility(string facilityID)
        {
            Facility facility;
            lock (Facilities)
            {
                if (facilityID == null)
                    return null;
                facility = Facilities.Where(x => x.FacilityID == facilityID).FirstOrDefault();
                if (facility == null)
                {
                    facility = new Facility() { FacilityID = facilityID };
                    Facilities.Add(facility);
                }
            }
            return facility;
        }
        public FlightPlan GetFlightPlan(Guid guid, string facilityID = null)
        {
            if (guid == Guid.Empty)
                return null;
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans = new List<FlightPlan>();
            lock (Facilities)
            {
                Facilities.ToList().ForEach(facility => flightPlans.AddRange(facility.FlightPlans));
            }
            lock (flightPlans)
            {
                flightPlan = (from x in flightPlans where x.Guid == guid select x).FirstOrDefault();
                if (flightPlan == null && facilityID != null)
                {
                    flightPlan = new FlightPlan(guid);
                    var facility = GetFacility(facilityID);
                    facility.FlightPlans.Add(flightPlan);
                }
            }
            return flightPlan;
        }
        public FlightPlan GetFlightPlan(string callsign, string facilityID)
        {
            if (facilityID == null || callsign == null)
                return null;
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans;
            Facility facility = GetFacility(facilityID);
            lock (facility.FlightPlans)
            {
                flightPlans = facility.FlightPlans.ToList();
                flightPlan = (from x in flightPlans where x.Callsign == callsign select x).FirstOrDefault();
                if (flightPlan == null)
                {
                    flightPlan = new FlightPlan(callsign);
                    facility.FlightPlans.Add(flightPlan);
                }
            }
            return flightPlan;
        }
        public void SendTrackUpdates(List<Track> tracks, TrackUpdate update)
        {
            tracks.ForEach(x => x.UpdateTrack(new TrackUpdate(update, x)));
        }
    }
}
