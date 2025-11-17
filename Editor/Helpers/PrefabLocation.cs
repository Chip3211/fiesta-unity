#if UNITY_EDITOR
using System;
using Newtonsoft.Json;

namespace Editor.Helpers
{
    [Serializable]
    public class PrefabLocation
    {
        [JsonProperty(Order = 7, PropertyName = "quarterW")]
        public readonly float QuarterW;

        [JsonProperty(Order = 4, PropertyName = "quarterX")]
        public readonly float QuarterX;

        [JsonProperty(Order = 5, PropertyName = "quarterY")]
        public readonly float QuarterY;

        [JsonProperty(Order = 6, PropertyName = "quarterZ")]
        public readonly float QuarterZ;

        [JsonProperty(Order = 8, PropertyName = "scale")]
        public readonly float Scale;

        [JsonProperty(Order = 1, PropertyName = "x")]
        public readonly float X;

        [JsonProperty(Order = 2, PropertyName = "y")]
        public readonly float Y;

        [JsonProperty(Order = 3, PropertyName = "z")]
        public readonly float Z;

        public PrefabLocation(float x, float y, float z, float quarterX, float quarterY, float quarterZ,
            float quarterW,
            float scale)
        {
            X = x;
            Y = y;
            Z = z;
            QuarterX = quarterX;
            QuarterY = quarterY;
            QuarterZ = quarterZ;
            QuarterW = quarterW;
            Scale = scale;
        }
    }
}
#endif