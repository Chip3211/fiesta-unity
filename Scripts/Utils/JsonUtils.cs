using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public abstract class JsonUtils
    {
        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T DeserializeFile<T>(string path)
        {
            var json = Resources.Load<TextAsset>(path).text;
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}