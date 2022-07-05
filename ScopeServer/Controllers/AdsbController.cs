using DGScope.Library;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Library.Adsb;

namespace ScopeServer.Controllers
{
    [Route("sendAdsb")]
    [ApiController]
    public class AdsbController : ControllerBase
    {
        private static Dictionary<int, CompactPositionReportingCoordinate> earlierPositionMessages = new Dictionary<int, CompactPositionReportingCoordinate>();
        private CompactPositionReporting cpr = new CompactPositionReporting();
        [HttpPost]
        public void Post([FromBody] AdsbUpdate[] value)
        {
            if (!Response.HttpContext.WebSockets.IsWebSocketRequest)
                Task.Run(() => ParseAdsbUpdates(value));
            else
                Console.WriteLine("Client at {0} tried a WebSocket, which is not implemented yet.", HttpContext.Connection.RemoteIpAddress);
        }

        

        public bool ParseAdsbUpdates(AdsbUpdate[] updates)
        {
            foreach (AdsbUpdate update in updates)
            {
                bool updated = false;
                ModeSMessage modes = update.ModeS;
                var adsb = update.Adsb;
                if (adsb != null)
                    modes = adsb.ModeSMessage;
                var facilities = Settings.Facilities.ToList();
                List<Track> tracks = new List<Track>();
                facilities.ForEach(facility =>
                {
                    lock (facility.Tracks)
                    {
                        tracks.AddRange(facility.Tracks.Where(track => track.ModeSCode == modes.Icao24));
                    }
                });
                if (tracks.Count == 0)
                    continue;
                var trackUpdate = new TrackUpdate();
                trackUpdate.TimeStamp = update.TimeStamp;
                if (modes.Altitude != null && modes.AltitudeIsMetric != true)
                {
                    trackUpdate.Altitude = new Altitude()
                    {
                        Value = (int)update.ModeS.Altitude,
                        AltitudeType = AltitudeType.Pressure
                    };
                    updated = true;
                }
                if (modes.Identity != null)
                {
                    trackUpdate.Squawk = modes.Identity.ToString().PadLeft(4, '0');
                    updated = true;
                }
                if (adsb != null)
                {
                    if (adsb.IdentifierAndCategory != null && adsb.IdentifierAndCategory.Identification != null)
                    {
                        trackUpdate.Callsign = adsb.IdentifierAndCategory.Identification.Trim();
                        updated = true;
                    }
                    if (adsb.AirbornePosition != null)
                    {
                        trackUpdate.IsOnGround = false;
                        updated = true;
                        GlobalCoordinate position = null;
                        if (adsb.AirbornePosition.CompactPosition != null)
                        {
                            lock (earlierPositionMessages)
                            {
                                if (earlierPositionMessages.ContainsKey(modes.Icao24))
                                {
                                    position = cpr.GlobalDecode(earlierPositionMessages[modes.Icao24], adsb.AirbornePosition.CompactPosition, new GlobalCoordinate(0, 0));
                                    if (position == null)
                                    {
                                        earlierPositionMessages[modes.Icao24] = adsb.AirbornePosition.CompactPosition;
                                    }
                                }
                                else
                                {
                                    earlierPositionMessages.Add(modes.Icao24, adsb.AirbornePosition.CompactPosition);
                                }
                            }
                            if (position != null)
                            {
                                trackUpdate.Location = new GeoPoint(position.Latitude, position.Longitude);
                            }
                        }
                        if (adsb.AirbornePosition.BarometricAltitude != null)
                        {
                            trackUpdate.Altitude = new Altitude()
                            {
                                Value = (int)adsb.AirbornePosition.BarometricAltitude,
                                AltitudeType = AltitudeType.Pressure
                            };
                        }
                    }
                    if (adsb.SurfacePosition != null)
                    {
                        trackUpdate.IsOnGround = true;
                        updated = true;
                    }
                    if (adsb.AirborneVelocity != null)
                    {
                        switch (adsb.AirborneVelocity.VelocityType)
                        {
                            case VelocityType.GroundSpeedSubsonic:
                                trackUpdate.GroundTrack = (int)adsb.AirborneVelocity.VectorVelocity.Bearing;
                                trackUpdate.GroundSpeed = (int)adsb.AirborneVelocity.VectorVelocity.Speed;
                                break;
                            default:
                                break;
                        }
                        updated = true;
                    }
                }
                if (update.LocalLocation != null)
                {
                    trackUpdate.Location = update.LocalLocation;
                    updated = true;
                }
                trackUpdate.Source = TrackUpdate.UpdateSource.ADS_B;
                if (updated)
                {
                    foreach (Track track in tracks)
                    {
                        var newUpdate = new TrackUpdate(trackUpdate, track);
                        track.UpdateTrack(newUpdate);
                    }
                }
            }
            return true;
        }
    }
}
