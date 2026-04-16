using CastleHero.Data.DB;
using UniRx;

namespace CastleHero.GamePlay.InGame
{
    public interface IWaveController
    {
        bool IsRunning { get; set; }
        ReactiveProperty<bool> Completed { get; }
        void Init(int groupId, IDBProvider db, IInGameSession inGameSession);
    }
}
