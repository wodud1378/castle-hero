using LitJson;
using Newtonsoft.Json;
using RGLabs.Network;

namespace RGLabs.Utility
{
    public static class NetworkHelper
    {
        private static readonly JsonSerializerSettings DefaultSetting = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public static string Text(this Error error) =>
            error switch
            {
                Error.Unknown => "알 수 없는 에러가 발생했습니다.",
                Error.Maintenance => "서버 점검 중입니다.",
                Error.FromNetwork => "네트워크 통신이 원활하지 않습니다.",
                Error.FromServer or Error.DBReadFailed or Error.DBWriteFailed => "서버 에러",
                _ => "잘못 된 요청입니다.",
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