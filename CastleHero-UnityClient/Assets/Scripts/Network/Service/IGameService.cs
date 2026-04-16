using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface IGameService
    {
        UniTask<Result<List<int>>> GetOpenedDungeonLayers();
        UniTask<Result> Start(GameType type, int id);
        UniTask<Result<GameCleared>> Clear(GameType type, int id);
    }
}
