using System;
using System.Collections.Generic;
using System.Text;

namespace DGScope.Library
{
    public class Track : IUpdatable
    {
        private double rateofturn;
        private DateTime lastLocationSetTime = DateTime.MinValue;
        private DateTime lastTrackUpdate = DateTime.MinValue;

        public int ModeSCode { get; private set; }
        public string Squawk { get; private set; }
        public GeoPoint Location { get; private set; }
        public string Callsign { get; private set; }
        public Altitude Altitude { get; }
        public int GroundSpeed { get; private set; }
        public int GroundTrack { get; private set; }
        public int VerticalRate { get; private set; }
        public bool Ident { get; private set; }
        public bool IsOnGround { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        
        public long LocationUpdateTime
        {
            get => lastLocationSetTime.ToFileTimeUtc();
        }
        public long GroundTrackUpdateTime
        {
            get => lastTrackUpdate.ToFileTimeUtc();
        }
        public Track(int modeS, Facility facility)
        {
            ModeSCode = modeS;
            Altitude = new Altitude(facility.Adaptation.TransitionAltitude, facility.Altimeter);
            Created?.Invoke(this, new TrackUpdatedEventArgs(GetCompleteTrackUpdate()));
        }
        public Track(Guid guid, Facility facility)
        {
            Guid = guid;
            Altitude = new Altitude(facility.Adaptation.TransitionAltitude, facility.Altimeter);
            Created?.Invoke(this, new TrackUpdatedEventArgs(GetCompleteTrackUpdate()));
        }
        public Track(Facility facility)
        {
            Altitude = new Altitude(facility.Adaptation.TransitionAltitude, facility.Altimeter);
            Created?.Invoke(this, new TrackUpdatedEventArgs(GetCompleteTrackUpdate()));
        }
        public bool SetGroundTrack (double Track, DateTime SetTime)
        {
            if (lastTrackUpdate > SetTime)
                return false;
            var diff = Track - GroundTrack;
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
            if (lastLocationSetTime > SetTime)
                return false;
            lastLocationSetTime = SetTime;
            this.Location = Location;
            return true;
        }
        private bool SetLocation(double Latitude, double Longitude, DateTime SetTime)
        {
            if (lastLocationSetTime > SetTime)
                return false;
            var newlocation = new GeoPoint(Latitude, Longitude);
            return SetLocation(newlocation, SetTime);
        }
        public double ExtrapolateTrack()
        {
            if (Math.Abs(rateofturn) > 5) // sanity check
            {
                return GroundTrack;
            }
            return (GroundTrack + (rateofturn / 2 * (DateTime.UtcNow - lastTrackUpdate).TotalSeconds) + 360) % 360;
        }

        public GeoPoint ExtrapolatePosition()
        {
            var miles = GroundSpeed * (DateTime.UtcNow - lastLocationSetTime).TotalHours;
            var track = ExtrapolateTrack();
            var location = Location.FromPoint(miles, track);
            return location;
        }
        public void UpdateTrack(TrackUpdate update)
        {
            update.RemoveUnchanged();
            if (update.TimeStamp < LastMessageTime)
                return;
            LastMessageTime = update.TimeStamp;
            bool changed = false;

            if (update.ModeSCode != null)
            {
                changed = true;
                this.ModeSCode = (int)update.ModeSCode;
            }
            if (update.Callsign != null)
            {
                changed = true;
                this.Callsign = update.Callsign;
            }
            if (update.Altitude != null)
            {
                changed = true;
                this.Altitude.TrueAltitude = update.Altitude.TrueAltitude;
            }
            if (update.GroundSpeed != null)
            {
                changed = true;
                this.GroundSpeed = (int)update.GroundSpeed;
            }
            if (update.GroundTrack != null)
            {
                changed = true;
                SetGroundTrack((double)update.GroundTrack, update.TimeStamp);
            }
            if (update.Ident != null)
            {
                changed = true;
                this.Ident = (bool)update.Ident;
            }
            if (update.IsOnGround != null)
            {
                changed = true;
                this.IsOnGround = (bool)update.IsOnGround;
            }
            if (update.Location != null)
            {
                changed = true;
                SetLocation(update.Location, update.TimeStamp);
            }
            if (update.VerticalRate != null)
            {
                changed = true;
                this.VerticalRate = (int)update.VerticalRate;
            }
            if (update.Squawk != null)
            {
                changed = true;
                this.Squawk = update.Squawk; 
            }
            if (changed)
                Updated?.Invoke(this, new TrackUpdatedEventArgs(update));
        }
        public TrackUpdate GetCompleteTrackUpdate()
        {
            var location = ExtrapolatePosition();
            var groundtrack = (int)ExtrapolateTrack();
            return new TrackUpdate(this)
            {
                Altitude = this.Altitude,
                Callsign = this.Callsign,
                TimeStamp = DateTime.UtcNow,
                GroundSpeed = this.GroundSpeed,
                GroundTrack = groundtrack,
                Ident = this.Ident,
                IsOnGround = this.IsOnGround,
                Location = location,
                VerticalRate = this.VerticalRate,
                Squawk = this.Squawk,
                ModeSCode = this.ModeSCode
            };

        }
        public override string ToString()
        {
            if (Callsign != null)
                return Callsign;
            return ModeSCode.ToString("X");
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
            Update = track.GetCompleteTrackUpdate();
        }
        public TrackUpdatedEventArgs(TrackUpdate update)
        {
            Track = update.Track;
            Update = update;
        }
    }
}
