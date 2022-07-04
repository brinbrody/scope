using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;
using Newtonsoft.Json;

namespace DGScope.Library
{
    public class AdsbUpdate
    {
        public ModeSMessage? ModeS { get; set; }
        public AdsbMessage? Adsb { get; set; }
        public GeoPoint? LocalLocation { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public AdsbUpdate() { }
        public AdsbUpdate(ModeSMessage modeS, AdsbMessage adsb, GeoPoint? localLocation = null)
        {
            ModeS = modeS;
            Adsb = adsb;
            LocalLocation = localLocation;
        }
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static AdsbUpdate DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject<AdsbUpdate>(json);
        }
    }
}
