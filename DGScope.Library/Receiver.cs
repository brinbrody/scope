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
                    track = new Track(icaoID);
                    facility.Tracks.Add(track);
                }
            }

            return track;
        }

        public Track GetTrack(Guid guid, string facilityID = null)
        {
            List<Track> tracks = new List<Track>();
            if (facilityID == null)
                Facilities.ToList().ForEach(facility => { lock (facility.Tracks) tracks.AddRange(facility.Tracks.Where(x => x.Guid == guid)); });
            else
            {
                var facility = GetFacility(facilityID);
                lock (facility.Tracks)
                {
                    facility.Tracks.ToList().ForEach(track => tracks.AddRange(facility.Tracks.Where(x => x.Guid == guid)));
                    Track track = tracks.FirstOrDefault();
                    if (track == null)
                    {
                        track = new Track(guid);
                        facility.Tracks.Add(track);
                        tracks.Add(track);
                    }
                }
            }
            return tracks.FirstOrDefault();
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
            var track = GetTrack(guid, facilityID);
            if (track == null)
                return null;
            return new List<Track>() { track };
        }
        public FlightPlan GetFlightPlan(Guid guid)
        {
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans = new List<FlightPlan>();
            Facilities.ToList().ForEach(facility => 
            { 
                lock (facility.FlightPlans) 
                    flightPlans.AddRange(facility.FlightPlans.Where(x => x.Guid == guid)); 
            });
            flightPlan = flightPlans.FirstOrDefault();
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
            Facility facility = null;
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans = new List<FlightPlan>();
            lock (Facilities)
            {
                Facilities.ToList().ForEach(facility => flightPlans.AddRange(facility.FlightPlans));
            }
            if (!string.IsNullOrEmpty(facilityID))
            {
                facility = GetFacility(facilityID);
                lock (facility.FlightPlans)
                {
                    flightPlan = (from x in facility.FlightPlans where x.Guid == guid select x).FirstOrDefault();
                    if (flightPlan == null)
                    {
                        flightPlan = new FlightPlan(guid);
                        facility.FlightPlans.Add(flightPlan);
                    }
                }
            }
            else
                flightPlan = (from x in flightPlans where x.Guid == guid select x).FirstOrDefault();
            return flightPlan;
        }
        public FlightPlan GetFlightPlan(string callsign, string facilityID)
        {
            if (string.IsNullOrEmpty(facilityID) || string.IsNullOrEmpty(callsign))
                return null;
            FlightPlan flightPlan;
            List<FlightPlan> flightPlans;
            Facility facility = GetFacility(facilityID);
            lock (facility.FlightPlans)
            {
                flightPlan = facility.FlightPlans.Where(x => x.Callsign == callsign).FirstOrDefault();
                if (flightPlan == null)
                {
                    flightPlan = new FlightPlan(callsign);
                    facility.FlightPlans.Add(flightPlan);
                }
                else
                    ;
            }
            return flightPlan;
        }
        public void SendTrackUpdates(List<Track> tracks, TrackUpdate update)
        {
            tracks.ForEach(x => x.UpdateTrack(new TrackUpdate(update, x)));
        }

        public void SendTrackUpdates(Track track, TrackUpdate update)
        {
            track.UpdateTrack(new TrackUpdate(update, track));
        }
    }
}
