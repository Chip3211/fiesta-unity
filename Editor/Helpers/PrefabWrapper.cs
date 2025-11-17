#if UNITY_EDITOR
using System;
using Newtonsoft.Json;

namespace Editor.Helpers
{
    [Serializable]
    public class PrefabWrapper
    {
        [JsonProperty(Order = 2, PropertyName = "coordinates")]
        public readonly PrefabLocation[] Coordinates;

        [JsonProperty(Order = 1, PropertyName = "path")]
        public readonly string Path;

        public PrefabWrapper(string path, PrefabLocation[] coordinates)
        {
            Path = path;
            Coordinates = coordinates;
        }
    }
}
#endif