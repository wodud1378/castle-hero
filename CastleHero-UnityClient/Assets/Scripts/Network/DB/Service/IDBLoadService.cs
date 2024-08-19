using Cysharp.Threading.Tasks;
using RGLabs.Common.Localize;
using RGLabs.Network.Service.Boot;

namespace RGLabs.Network.DB.Service
{
    public interface IDBLoadService
    {
        public UniTask<(DBCollections db, LocalizeText localize)> InitialLoad(ChartInfo[] chartInfo);
    }
}