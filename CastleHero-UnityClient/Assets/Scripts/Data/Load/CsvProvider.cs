using System.Reflection;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Load
{
    public class CsvProvider
    {
        public async UniTask<string> LoadCsvText<T>()
        {
            var type = typeof(T);
            var att = type.GetCustomAttribute<DBAttribute>();

            return await LoadCsvText(att);
        }
        
        public async UniTask<string> LoadCsvText(DBAttribute attribute)
        {
            var asset = await Resources.LoadAsync<TextAsset>(string.Empty);
            if (asset == null)
                return string.Empty;

            return (asset as TextAsset)!.ToString();
        }
    }
}