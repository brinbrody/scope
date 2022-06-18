using System;
using System.Collections.Generic;
using System.Text;

namespace DGScope.Library
{
    public class Track
    {
        public string squawk;
        public string callsign;
        private double rateofturn;
        private int groundspeed;
        private int verticalrate;
        private bool ident;
        private bool isOnGround;
        private DateTime lastLocationSetTime = DateTime.MinValue;
        private DateTime lastTrackUpdate = DateTime.MinValue;

        public int ModeSCode { get; }
        public string Squawk 
        { 
            get => squawk;
            set
            {
                if (squawk == value)
                    return;
                squawk = value;
                Updated?.Invoke(this, new EventArgs());
            }
        }
        public GeoPoint Location { get; private set; }
        public string ModeSCallsign 
        {
            get => callsign;
            set
            {
                if (callsign == value)
                    return;
                callsign = value;
                Updated?.Invoke(this, new EventArgs());
            }
        }
        public Altitude Altitude { get; } = new Altitude();
        public int GroundSpeed 
        {
            get => groundspeed;
            set
            {
                if (groundspeed == value)
                    return;
                groundspeed = value;
                Updated?.Invoke(this, new EventArgs());
            }
        }
        public int GroundTrack 
        { get; 
            private set; }
        public int VerticalRate 
        {
            get => verticalrate;
            set
            {
                if (verticalrate == value)
                    return;
                verticalrate = value;
                Updated?.Invoke(this, new EventArgs());
            }
        }
        public bool Ident 
        {
            get => ident;
            set
            {
                if (ident == value)
                    return;
                ident = value;
                Updated?.Invoke(this, new EventArgs());
            }
        }
        public bool IsOnGround
        {
            get => isOnGround;
            set
            {
                if (isOnGround == value)
                    return;
                isOnGround = value;
                Updated?.Invoke(this, new EventArgs());
            }
        }
        public DateTime LastMessageTime { get; private set; }
        public long LocationUpdateTime
        {
            get => lastLocationSetTime.ToFileTimeUtc();
        }
        public long GroundTrackUpdateTime
        {
            get => lastTrackUpdate.ToFileTimeUtc();
        }
        public Track(int modeS)
        {
            ModeSCode = modeS;
            Created.Invoke(this, new EventArgs());
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
            Updated?.Invoke(this, new EventArgs());
            lastTrackUpdate = SetTime;
            return true;
        }
        public bool SetLocation(GeoPoint Location, DateTime SetTime)
        {
            if (lastLocationSetTime > SetTime)
                return false;
            lastLocationSetTime = SetTime;
            this.Location = Location;
            Updated?.Invoke(this, new EventArgs());
            return true;
        }
        public bool SetLocation(double Latitude, double Longitude, DateTime SetTime)
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
        public event EventHandler Updated;
        public event EventHandler Created;
    }
    
}
