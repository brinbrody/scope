using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public abstract class Update
    {
        public abstract Guid Guid { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;
        public abstract UpdateType UpdateType { get; }
        public abstract void RemoveUnchanged();
        public static string SerializeToJson(Update update)
        {
            return JsonConvert.SerializeObject(update, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
        public string SerializeToJson()
        {
            return SerializeToJson(this);
        }
        public async Task<string> SerializeToJsonAsync()
        {
            return await JsonConvert.SerializeObjectAsync(this, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
        public Update DeserializeFromJson(string json)
        {
            return null;
        }
    }
    public class UpdateConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Update));
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Update update;
            switch (jo["UpdateType"].Value<UpdateType>())
            {
                case UpdateType.Track:
                    update = JsonConvert.DeserializeObject<TrackUpdate>(jo.ToString(), SpecifiedSubclassConversion);
                    var track = (update as TrackUpdate).Track;
                    return new TrackUpdate(update as TrackUpdate, track);
                case UpdateType.Flightplan:
                    return JsonConvert.DeserializeObject<FlightPlanUpdate>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new NotImplementedException("Unknown update type");
            }
            throw new NotImplementedException();
        }
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Update).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class UpdateEventArgs : EventArgs
    {
        public Update Update { get; protected set; }
        public UpdateEventArgs(Update update)
        {
            Update = update;
        }
        public UpdateEventArgs() { }
    }
    public enum UpdateType
    {
        Track,
        Flightplan
    }
}
