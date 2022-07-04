﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace DGScope.Library
{
    public class TrackUpdate : Update
    {
        public UpdateSource Source { get; set; }
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

        public override UpdateType UpdateType => UpdateType.Track;

        public TrackUpdate(Track track, DateTime timestamp)
        {
            Base = track;
            if (track.Altitude != null)
                Altitude = track.Altitude.Clone();
            TimeStamp = timestamp;
        }
        public TrackUpdate(Track track)
        {
            Base = track;
            if (track.Altitude != null)
                Altitude = track.Altitude.Clone();
        }
        public TrackUpdate() { }
        public TrackUpdate(TrackUpdate trackUpdate, Track track)
        {
            SetAllProperties(trackUpdate);
            Base = track;
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
            return DeserializeFromJson(json, Base as Track);
        }
        public new void RemoveUnchanged()
        {
            base.RemoveUnchanged();
            //var track = Base as Track;
            //    ModeSCode = track.ModeSCode;
        }
        public enum UpdateSource
        {
            FAA_STDDS,
            ADS_B
        }
    }
}
