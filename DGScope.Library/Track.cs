using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DGScope.Library
{
    public class Track : IUpdatable
    {
        private double rateofturn;
        //private DateTime lastLocationSetTime = DateTime.MinValue;
        //private DateTime lastTrackUpdate = DateTime.MinValue;

        public int? ModeSCode { get; private set; }
        public string? Squawk { get; private set; }
        public GeoPoint Location { get; private set; }
        public string? Callsign { get; private set; }
        public Altitude Altitude { get; private set; }
        public int? GroundSpeed { get; private set; }
        public int? GroundTrack { get; private set; }
        public int? VerticalRate { get; private set; }
        public bool? Ident { get; private set; }
        public bool? IsOnGround { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        
        public long LocationUpdateTime
        {
            get
            {
                DateTime lastLocationSetTime;
                if (!PropertyUpdatedTimes.TryGetValue("Location", out lastLocationSetTime))
                {
                    lastLocationSetTime = DateTime.MinValue;
                }
                if (lastLocationSetTime == DateTime.MinValue)
                    return 0;
                return lastLocationSetTime.ToFileTimeUtc();
            }
        }
        public long GroundTrackUpdateTime
        {
            get
            {
                DateTime lastTrackUpdate;
                if (!PropertyUpdatedTimes.TryGetValue("GroundTrack", out lastTrackUpdate))
                { 
                    lastTrackUpdate = DateTime.MinValue;
                }
                if (lastTrackUpdate == DateTime.MinValue)
                    return 0;
                return lastTrackUpdate.ToFileTimeUtc();
            }
        }

        public Dictionary<string, DateTime> PropertyUpdatedTimes { get; } = new Dictionary<string, DateTime>();

        public Track(int modeS)
        {
            ModeSCode = modeS;
            Created?.Invoke(this, new TrackUpdatedEventArgs(GetCompleteUpdate()));
        }
        public Track(Guid guid)
        {
            Guid = guid;
            Created?.Invoke(this, new TrackUpdatedEventArgs(GetCompleteUpdate()));
        }
        public Track(Facility facility)
        {
            Created?.Invoke(this, new TrackUpdatedEventArgs(GetCompleteUpdate()));
        }
        public bool SetGroundTrack (double Track, DateTime SetTime)
        {
            DateTime lastTrackUpdate;
            if (!PropertyUpdatedTimes.TryGetValue("GroundTrack", out lastTrackUpdate))
                lastTrackUpdate = DateTime.MinValue;
            if (lastTrackUpdate > SetTime)
                return false;
            if (GroundTrack == null)
                GroundTrack = (int)Track;
            var diff = Track - (int)GroundTrack;
            if (Math.Abs(diff) > 180)
            {
                if (diff > 0)
                    diff = 360 - diff;
                else
                    diff += 360;
            }
            var seconds = (SetTime - lastTrackUpdate).TotalSeconds;
            GroundTrack = (int)Track;
            if (seconds == 0)
                return false;
            rateofturn = diff / seconds;
            lastTrackUpdate = SetTime;
            return true;
        }
        private bool SetLocation(GeoPoint Location, DateTime SetTime)
        {
            DateTime lastLocationSetTime;
            if (!PropertyUpdatedTimes.TryGetValue("Location", out lastLocationSetTime))
                lastLocationSetTime = DateTime.MinValue;
            if (lastLocationSetTime > SetTime)
                return false;
            this.Location = Location;
            if (!PropertyUpdatedTimes.TryAdd("Location", SetTime))
                PropertyUpdatedTimes["Location"] = SetTime;
            return true;
        }
        private bool SetLocation(double Latitude, double Longitude, DateTime SetTime)
        {
            var newlocation = new GeoPoint(Latitude, Longitude);
            return SetLocation(newlocation, SetTime);
        }
        public double? ExtrapolateTrack()
        {
            DateTime lastTrackUpdate;
            if (!PropertyUpdatedTimes.TryGetValue("Track", out lastTrackUpdate))
                return null;
            if (Math.Abs(rateofturn) > 5) // sanity check
            {
                return GroundTrack;
            }
            return (GroundTrack + (rateofturn / 2 * (DateTime.UtcNow - lastTrackUpdate).TotalSeconds) + 360) % 360;
        }

        public GeoPoint ExtrapolatePosition()
        {
            if (Location == null)
                return null;
            DateTime lastLocationSetTime;
            if (!PropertyUpdatedTimes.TryGetValue("Location", out lastLocationSetTime))
                lastLocationSetTime = DateTime.MinValue;
            if (GroundSpeed == null)
                return Location;
            var miles = (int)GroundSpeed * (DateTime.UtcNow - lastLocationSetTime).TotalHours;
            var track = ExtrapolateTrack();
            if (track == null)
                return null;
            var location = Location.FromPoint(miles, (double)track);
            return location;
        }
        public void UpdateTrack(TrackUpdate update)
        {
            //update.RemoveUnchanged();
            if (update.Source == TrackUpdate.UpdateSource.ADS_B)
                ;

            bool changed = false;
            foreach (var updateProperty in update.GetType().GetProperties())
            {
                PropertyInfo thisProperty = GetType().GetProperty(updateProperty.Name);
                object updateValue = updateProperty.GetValue(update);
                if (updateValue == null || thisProperty == null)
                    continue;
                if (!PropertyUpdatedTimes.TryGetValue(thisProperty.Name, out DateTime lastUpdatedTime))
                    PropertyUpdatedTimes.TryAdd(thisProperty.Name, update.TimeStamp);
                if (update.TimeStamp > lastUpdatedTime)
                {
                    if (thisProperty.CanWrite)
                    {
                        thisProperty.SetValue(this, updateValue);
                        PropertyUpdatedTimes[thisProperty.Name] = update.TimeStamp;
                        changed = true;
                        if (updateProperty.Name == "GroundTrack")
                            SetGroundTrack((double)update.GroundTrack, update.TimeStamp);
                        else if (updateProperty.Name == "Location")
                            SetLocation(update.Location, update.TimeStamp);
                    }
                    else
                    {
                        switch (thisProperty.Name)
                        {
                            

                        }
                    }
                }
            }
            if (changed)
            {
                if (update.TimeStamp > LastMessageTime)
                    LastMessageTime = update.TimeStamp;
                Updated?.Invoke(this, new TrackUpdatedEventArgs(update));
            }
            return;
            
        }
        public Update GetCompleteUpdate()
        {
            var location = ExtrapolatePosition();
            var groundtrack = ExtrapolateTrack();
            if (groundtrack != null)
                groundtrack = (int)groundtrack;
            var newUpdate = new TrackUpdate(this);
            newUpdate.SetAllProperties();
            newUpdate.Location = location;
            if (groundtrack != null)
                newUpdate.GroundTrack = (int)groundtrack;
            else
            {
                newUpdate.GroundTrack = GroundTrack;
            }
            newUpdate.TimeStamp = DateTime.Now;
            return newUpdate;
        }
        public Update GetUpdate(Update update)
        {
            if (update.GetType() != typeof(FlightPlanUpdate))
                throw new InvalidCastException();
            update.Base = this;
            foreach (PropertyInfo updateProperty in update.GetType().GetProperties())
            {
                PropertyInfo thisProperty = GetType().GetProperty(updateProperty.Name);
                if (!PropertyUpdatedTimes.TryGetValue(thisProperty.Name, out DateTime updated))
                    updated = DateTime.MinValue;
                object thisValue = thisProperty.GetValue(this);
                object updateValue = updateProperty.GetValue(update);
                if (update.TimeStamp < updated)
                    updateProperty.SetValue(update, null);
            }
            return update;
        }
        public override string ToString()
        {
            return Guid.ToString();
        }
        public event EventHandler<UpdateEventArgs> Updated;
        public event EventHandler<UpdateEventArgs> Created;
    }
    public class TrackUpdatedEventArgs : UpdateEventArgs
    {
        public Track Track { get; private set; }
        public TrackUpdatedEventArgs(Track track)
        {
            Track = track;
            Update = track.GetCompleteUpdate();
        }
        public TrackUpdatedEventArgs(Update update)
        {
            Track = update.Base as Track;
            Update = update;
        }
    }
}
