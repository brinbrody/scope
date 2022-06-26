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
    public class ReceiverList : List<Receiver>
    {
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
        public ReceiverList() : base() { }
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this, serializerSettings);
        }
        public static ReceiverList DeserializerFromJson(string json)
        {
            return JsonConvert.DeserializeObject(json, serializerSettings) as ReceiverList;
        }
    }
    public class ReceiverConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Receiver));
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Type type = Type.GetType(jo["AssemblyQualifiedName"].Value<string>());
            if (type == typeof(Receiver))
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
            throw new Exception();
            
        }
    }
}
