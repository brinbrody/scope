using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Linq;
using System.IO;

namespace DGScope.Library
{
    public class VideoMapList : List<VideoMap>
    {
        [XmlIgnore]
        public string Filename { get; set; }
        public VideoMapList() : base() { }
        public VideoMapList(string filename) : base()
        {
            Filename = filename;
        }
        public new void Add(VideoMap map) 
        {
            while (Contains(map))
            {
                map.Number++;
            }
            base.Add(map);
        }

        public new void AddRange(IEnumerable<VideoMap> collection)
        {
            foreach (var map in collection)
            {
                Add(map);
            }
        }
        public static void SerializeToJsonFile(VideoMapList videoMaps, string filename)
        {
            File.WriteAllText(filename, SerializeToJson(videoMaps));
        }
        public static string SerializeToJson(VideoMapList videoMaps)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(videoMaps, options);
        }
        public static VideoMapList DeserializeFromJsonFile(string filename)
        {
            string json = File.ReadAllText(filename);
            return DeserializeFromJson(json);
        }
        public static VideoMapList DeserializeFromJson(string jsonString)
        {
            return (VideoMapList)JsonSerializer.Deserialize(jsonString,typeof(VideoMapList));
        }
    }
}
