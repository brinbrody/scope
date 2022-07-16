using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Library;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Library.Adsb;
using VirtualRadar.Library.ModeS;
using VirtualRadar.Library.Listener;
using System.Diagnostics;
using DGScope.Library;
using VirtualRadar.Interface.ModeS;

namespace DGScope.Receivers.Beast
{
    public class BeastReceiver : TcpClientReceiver
    {
        private BeastMessageBytesExtractor extractor = new BeastMessageBytesExtractor();
        private ModeSTranslator mst;
        private AdsbTranslator adsbt;
        private GlobalCoordinate coordinates => new GlobalCoordinate();
        private CompactPositionReporting cpr = new CompactPositionReporting();
        private Dictionary<int, CompactPositionReportingCoordinate> earlierPositionMessages = new Dictionary<int, CompactPositionReportingCoordinate>();
        public GeoPoint ReceiverLocation { get; set; } = new GeoPoint();
        public BeastReceiver()
        {
            InterfaceFactory.Factory.Register(typeof(IBitStream), typeof(BitStream));
            mst = new ModeSTranslator();
            adsbt = new AdsbTranslator();
            mst.Statistics = new Statistics();
            adsbt.Statistics = mst.Statistics;
        }
        protected override void DataReceived(byte[] buffer)
        {
            var bytes = extractor.ExtractMessageBytes(buffer, 0, buffer.Length);
            foreach (ExtractedBytes item in bytes)
            {
                var modes = mst.Translate(item.Bytes, 0, item.SignalLevel, item.IsMlat);
                if (modes.Icao24 == 0)
                    continue;
                var adsb = adsbt.Translate(modes);
                AdsbUpdate update;
                if (adsb != null)
                {
                    GeoPoint location = null;
                    if (adsb.AirbornePosition != null && adsb.AirbornePosition.CompactPosition != null && ReceiverLocation != null)
                    {
                        coordinates.Latitude = ReceiverLocation.Latitude;
                        coordinates.Longitude = ReceiverLocation.Longitude;
                        var loc = cpr.LocalDecode(adsb.AirbornePosition.CompactPosition, coordinates);
                        location = new GeoPoint(loc.Latitude, loc.Longitude);
                    }
                    update = new AdsbUpdate(null, adsb, location);
                }
                else
                    update = new AdsbUpdate(modes, adsb);
                var trackUpdate = ParseAdsbUpdate(update).Result;
                SendUpdate(trackUpdate);
            }
        }

        public async Task<TrackUpdate> ParseAdsbUpdate(AdsbUpdate update)
        {
            ModeSMessage modes = update.ModeS;
            var adsb = update.Adsb;
            if (adsb != null)
                modes = adsb.ModeSMessage;
            var trackUpdate = new TrackUpdate();
            trackUpdate.TimeStamp = update.TimeStamp;
            if (modes.Altitude != null && modes.AltitudeIsMetric != true)
            {
                trackUpdate.Altitude = new Altitude()
                {
                    Value = (int)update.ModeS.Altitude,
                    AltitudeType = AltitudeType.Pressure
                };
            }
            if (modes.Identity != null)
            {
                trackUpdate.Squawk = modes.Identity.ToString().PadLeft(4, '0');
            }
            if (adsb != null)
            {
                if (adsb.IdentifierAndCategory != null && adsb.IdentifierAndCategory.Identification != null)
                {
                    trackUpdate.Callsign = adsb.IdentifierAndCategory.Identification.Trim();
                }
                if (adsb.AirbornePosition != null)
                {
                    trackUpdate.IsOnGround = false;
                    GlobalCoordinate position = null;
                    if (adsb.AirbornePosition.CompactPosition != null)
                    {
                        if (ReceiverLocation != null)
                        {
                            coordinates.Latitude = ReceiverLocation.Latitude;
                            coordinates.Longitude = ReceiverLocation.Longitude;
                        }
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
                }
            }
            if (update.LocalLocation != null)
            {
                trackUpdate.Location = update.LocalLocation;
            }
            trackUpdate.Source = TrackUpdate.UpdateSource.ADS_B;
            return trackUpdate;
        }
    }
}
