using CastleHero.Data.Model;
using CastleHero.Network.Shared;

namespace CastleHero.GamePlay.InGame.Behaviours
{
    public struct GameResult
    {
        public bool IsCleared;
        public int Id;
        public GameType Type;
        public GameCleared Data;
    }
}
