using LitJson;
using Newtonsoft.Json;

namespace RGLabs.Utility
{
    public static class NetworkHelper
    {
        private static readonly JsonSerializerSettings DefaultSetting = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        
        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, DefaultSetting);

        public static T Cast<T>(this JsonData data)
        {
            var str = data.ToJson();
            if (str.StartsWith("\""))
                str = str.Remove(0, 1);
            if (str.EndsWith("\""))
                str = str.Remove(str.Length - 1, 1);
            
            str = str
                .Replace("BackendFunction", "Assembly-CSharp")
                .Replace("\\", string.Empty);

            return JsonConvert.DeserializeObject<T>(str, DefaultSetting);
        }

        public static int ToInt(this JsonData data) => int.Parse(data.ToString());

        public static float ToFloat(this JsonData data) => float.Parse(data.ToString());
    }
}