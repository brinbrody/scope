using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class TrackUpdate : Update
    {
        private Guid trackGuid = Guid.NewGuid();
        [JsonIgnore]
        public Track Track { get; private set; }
        public Altitude? Altitude { get; set; }
        public int? GroundSpeed { get; set; }
        public int? GroundTrack { get; set; }
        public bool? Ident { get; set; }
        public bool? IsOnGround { get; set; }
        public string? Squawk { get; set; }
        public GeoPoint? Location { get; set; }
        public string? Callsign { get; set; }
        public int? VerticalRate { get; set; }
        public int? ModeSCode { get; set; }
        public override Guid Guid
        {
            get
            {
                if (Track != null)
                    return Track.Guid;
                return trackGuid;
            }
            set
            {
                trackGuid = value;
            }
        }

        public override UpdateType UpdateType => UpdateType.Track;

        public TrackUpdate(Track track, DateTime timestamp)
        {
            Track = track;
            Altitude = track.Altitude.Clone();
            TimeStamp = timestamp;
        }
        public TrackUpdate(Track track)
        {
            Track = track;
            Altitude = track.Altitude.Clone();
        }
        public TrackUpdate() { Altitude = new Altitude(); }
        public TrackUpdate(TrackUpdate trackUpdate, Track track)
        {
            Track = track;
            TimeStamp = trackUpdate.TimeStamp;
            Altitude = trackUpdate.Altitude;
            GroundTrack = trackUpdate.GroundTrack;
            GroundSpeed = trackUpdate.GroundSpeed;
            Ident = trackUpdate.Ident;
            IsOnGround = trackUpdate.IsOnGround;
            Squawk = trackUpdate.Squawk;
            Location = trackUpdate.Location;
            Callsign = trackUpdate.Callsign;
            VerticalRate = trackUpdate.VerticalRate;
            ModeSCode = trackUpdate.ModeSCode;
        }

        public string SerializeToJson()
        {
            return SerializeToJson(this);
        }

        public static string SerializeToJson(TrackUpdate trackUpdate)
        {
            return JsonConvert.SerializeObject(trackUpdate, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static TrackUpdate DeserializeFromJson(string json, Track track)
        {
            var update = JsonConvert.DeserializeObject<TrackUpdate>(json);
            return new TrackUpdate(update, track);
        }

        public Update DeserializeFromJson(string json)
        {
            return DeserializeFromJson(json, this.Track);
        }
        public override void RemoveUnchanged()
        {
            if (Altitude == null || Altitude.Equals(Track.Altitude))
                Altitude = null;
            if (GroundSpeed == Track.GroundSpeed)
                GroundSpeed = null;
            if (GroundTrack == Track.GroundTrack)
                GroundTrack = null;
            if (Ident == Track.Ident)
                Ident = null;
            if (IsOnGround == Track.IsOnGround)
                IsOnGround = null;
            if (Squawk == Track.Squawk)
                Squawk = null;
            if (Location == null || Location.Equals(Track.Location))
                Location = null;
            if (Callsign == Track.Callsign)
                Callsign = null;
            if (VerticalRate == Track.VerticalRate)
                VerticalRate = null;
        }
    }
}
