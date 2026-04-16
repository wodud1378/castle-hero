using Cysharp.Threading.Tasks;
using CastleHero.Common.Localize;
using CastleHero.Network.Service.Boot;

namespace CastleHero.Network.DB.Service
{
    public interface IDBLoadService
    {
        public UniTask<DBCollections> InitialLoad(ChartInfo[] chartInfo);
    }
}