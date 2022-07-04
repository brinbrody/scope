using DGScope.Receivers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.AdsbUploadClient
{
    class AdsbUploadClientSettings
    {
        public AdsbReceiverList Receivers { get; set; }
        public string UploadUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string SerializeToJson()
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.Indented };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static AdsbUploadClientSettings DeserializeFromJson(string json)
        {
            //JsonConverter[] converters = new JsonConverter[] { new AdsbReceiverConverter() };
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.DeserializeObject<AdsbUploadClientSettings>(json, settings);
        }

    }
}
