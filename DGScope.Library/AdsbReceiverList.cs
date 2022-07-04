using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers
{
    public class AdsbReceiverList : List<AdsbReceiver>
    {
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
        public AdsbReceiverList() : base() { }
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this, serializerSettings);
        }
        public static AdsbReceiverList DeserializerFromJson(string json)
        {
            return JsonConvert.DeserializeObject(json, serializerSettings) as AdsbReceiverList;
        }
    }
    public class AdsbReceiverConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsSubclassOf(typeof(AdsbReceiver)))
                return true;
            if (objectType == typeof(AdsbReceiver))
                return true;
            return false;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Type type = Type.GetType(jo["$type"].Value<string>());
            if (type.IsSubclassOf(typeof(AdsbReceiver)) || (type == typeof(AdsbReceiver)))
            {
                return serializer.Deserialize(reader, type);
            }
            return null;
        }
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var newserializer = new JsonSerializer();
            newserializer.TypeNameHandling = TypeNameHandling.All;
            newserializer.Serialize(writer, value);

        }
    }
}
