namespace CastleHero.Data.Model
{
    public struct GameFinished
    {
        public bool IsCleared;
        public GameEvent Cause;
        public GameType Type;
        public int Id;
    }
}
