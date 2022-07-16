using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.ReadsbJSON
{
    public class ReadsbJSONMessage
    {
        public double now { get; set; }
        public string hex { get; set; }
        public string type { get; set; }
        public string flight { get; set; }
        public object alt_baro { get; set; }
        public int? alt_geom { get; set; }
        public float? gs { get; set; }
        public float? track { get; set; }
        public int? baro_rate { get; set; }
        public string squawk { get; set; }
        public string emergency { get; set; }
        public string category { get; set; }
        public float? nav_qnh { get; set; }
        public int? nav_altitude_mcp { get; set; }
        public float? lat { get; set; }
        public float? lon { get; set; }
        public int? nic { get; set; }
        public int? rc { get; set; }
        public double? seen_pos { get; set; }
        public int version { get; set; }
        public int nic_baro { get; set; }
        public int? nac_p { get; set; }
        public int? nac_v { get; set; }
        public int? sil { get; set; }
        public string sil_type { get; set; }
        public int? gva { get; set; }
        public int? sda { get; set; }
        public int? alert { get; set; }
        public int? spi { get; set; }
        public object[] mlat { get; set; }
        public object[] tisb { get; set; }
        public int? messages { get; set; }
        public double seen { get; set; }
        public float rssi { get; set; }
        public int? geom_rate { get; set; }
        public float? nav_heading { get; set; }
        public string[] nav_modes { get; set; }
        public float? true_heading { get; set; }
        public DateTime TimeStamp 
        { 
            get
            {
                var now = DateTime.UnixEpoch.AddSeconds(this.now);
                return now.AddSeconds(-this.seen);
            } 
        }
        public DateTime PositionTimeStamp
        {
            get
            {
                if (this.seen_pos == null)
                    throw new NullReferenceException("seen_pos was null");
                var now = DateTime.UnixEpoch.AddSeconds(this.now);
                if (double.TryParse(this.seen_pos.ToString(), out double seen))
                    return now.AddSeconds(-seen);
                throw new InvalidCastException("seen_pos was not valid");
            }
        }
        public int Modes
        {
            get
            {
                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int modes))
                    return modes;
                return 0;
            }
        }

        public static ReadsbJSONMessage FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ReadsbJSONMessage>(json);
        }
    }

}
