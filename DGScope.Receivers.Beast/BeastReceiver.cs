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

namespace DGScope.Receivers.Beast
{
    public class BeastReceiver : TcpClientReceiver
    {
        private BeastMessageBytesExtractor extractor = new BeastMessageBytesExtractor();
        private ModeSTranslator mst;
        private AdsbTranslator adsbt;
        private GlobalCoordinate coordinates = new GlobalCoordinate();
        private CompactPositionReporting cpr = new CompactPositionReporting();
        public GeoPoint ReceiverLocation { get; set; }
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
                if (modes.Icao24 == null)
                    continue;
                var adsb = adsbt.Translate(modes);
                AdsbUpdate update;
                if (adsb != null)
                {
                    GeoPoint location = null;
                    if (adsb.AirbornePosition != null && adsb.AirbornePosition.CompactPosition != null  && ReceiverLocation != null)
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
                SendUpdate(update);
            }
        }
    }
}
