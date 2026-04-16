namespace CastleHero.Common.Flow
{
    // Scene-level state. Defined in Domain so Storage can reference it without importing Common.
    public enum State
    {
        None,
        Lobby,
        InGame,
    }
}
