using System.Collections.Generic;
using System;
using static DGScope.Library.Constants;
namespace DGScope.Library
{
    public class RadarSite
    {
        public char ID { get; set; }
        public string Name { get; set; }
        public GeoPoint Location { get; set; } = new GeoPoint();
        public int Elevation { get; set; } = 0;
        public int PrimaryRange { get; set; } = 100;
        public int SecondaryRange { get; set; } = 100;
        public double MinElevation { get; set; } = 90;
        public double MaxElevation { get; set; } = -90;
        public SiteType SiteType { get; set; } = SiteType.FUSED;
        public bool Rotating { get; set; } = false;
        public double UpdateRate { get; set; } = 1.0;
        public GeoPoint? TrackLocation (Track track)
        {
            GeoPoint? output = null;
            var track_location = track.ExtrapolatePosition();
            double range = track_location.DistanceTo(Location);
            double elevation = Math.Atan2((track.Altitude.TrueAltitude - Elevation) / NAUTICAL_MILE_IN_FEET, range);
            if (SiteType == SiteType.SINGLE_SITE)
            {
                var slant_range = track_location.DistanceTo(Location, track.Altitude.TrueAltitude - Elevation);
                if (slant_range <= PrimaryRange)
                    output = Location.FromPoint(slant_range, Location.BearingTo(track_location));
            }
            else if (SiteType == SiteType.FUSED)
            {
                if (range <= PrimaryRange)
                    output = track.Location;
            }
            else if (SiteType == SiteType.MULTI_SITE)
            {
                if (Sites.Count >= 2)
                    foreach (var site in Sites)
                    {
                        var location = site.TrackLocation(track);
                        if (location != null)
                            return track.Location;
                    }
                else if (Sites.Count == 1)
                {
                    SiteType = SiteType.SINGLE_SITE;
                    output = Sites[0].TrackLocation(track);
                }
            }
            return output;
        }
        public List<RadarSite> Sites = new List<RadarSite>();
    }
    public enum SiteType
    {
        SINGLE_SITE,
        MULTI_SITE,
        FUSED
    }
}
