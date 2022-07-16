using DGScope.Library;
using DGScope.Receivers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.ReadsbJSON
{
    public class ReadsbJSONReceiver : TcpClientReceiver
    {
        private QueueStream stream = new QueueStream();
        public ReadsbJSONReceiver()
        {
            
        }
        protected override void DataReceived(byte[] rx)
        {
            var json = Encoding.UTF8.GetString(rx);
            foreach (var line in json.Split('\n'))
            {
                ProcessLine(line);
            }
        }

        protected override void cbConnectComplete()
        {
            base.cbConnectComplete();
            while (Connected)
            {
                string partialline = "";
                using (StreamReader reader = new StreamReader(Stream,Encoding.UTF8,false,-1,true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(partialline))
                        {
                            line = partialline + line;
                        }
                        if (line[line.Length - 1] == '}')
                        {
                            ProcessLine(line);
                            partialline = "";
                        }
                        else
                        { 
                            partialline += line;
                        }
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        private async Task ProcessLine(string line)
        {
            var adsb = ReadsbJSONMessage.FromJson(line);
            if (adsb == null)
                return;
            var update = new TrackUpdate()
            {
                Source = TrackUpdate.UpdateSource.ADS_B
            };
            if (adsb.Modes == 0)
                return;
            update.TimeStamp = adsb.TimeStamp;
            update.ModeSCode = adsb.Modes;
            if (!string.IsNullOrWhiteSpace(adsb.flight))
                update.Callsign = adsb.flight.Trim();
            
            if (adsb.alt_baro != null && int.TryParse(adsb.alt_baro.ToString(), out int alt))
            {
                update.Altitude = new Altitude()
                {
                    Value = alt,
                    AltitudeType = AltitudeType.Pressure
                };
                update.IsOnGround = false;
            }
            else if(adsb.alt_baro != null && adsb.alt_baro.ToString() == "ground")
            {
                update.IsOnGround = true;
            }
            else if (adsb.alt_baro != null && int.TryParse(adsb.alt_baro.ToString(), out alt))
            {
                update.Altitude = new Altitude()
                {
                    Value = alt,
                    AltitudeType = AltitudeType.True
                };
            }

            if (adsb.gs != null)
                update.GroundSpeed = (int)adsb.gs;

            if (adsb.track != null)
                update.GroundTrack = (int)adsb.track;

            if (adsb.baro_rate != null)
                update.VerticalRate = adsb.baro_rate;
            else if (adsb.geom_rate != null)
                update.VerticalRate = adsb.geom_rate;

            if (adsb.squawk != null)
                update.Squawk = adsb.squawk;

            if (adsb.lat != null && adsb.lon != null)
            {
                update.Location = new GeoPoint((double)adsb.lat, (double)adsb.lon);
                update.TimeStamp = adsb.PositionTimeStamp;
            }

            update.Ident = adsb.spi == 1;
            SendUpdate(update);
        }
        
    }
}
