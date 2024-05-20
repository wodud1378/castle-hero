using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Load
{
    public interface ICsvProvider
    {
        public UniTask<string> LoadCsvText(DBAttribute attribute);
    }

    public class LocalCsvProvider: ICsvProvider
    {
        public async UniTask<string> LoadCsvText(DBAttribute attribute)
        {
            var asset = await Resources.LoadAsync<TextAsset>(attribute.Path);
            if (asset == null)
                return string.Empty;

            return (asset as TextAsset)!.ToString();
        }
    }
}