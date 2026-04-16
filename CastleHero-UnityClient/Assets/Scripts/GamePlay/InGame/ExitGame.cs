using CastleHero.Data.Model;

namespace CastleHero.GamePlay.InGame
{
    public enum ExitCode
    {
        Exit,
        Retry,
        Next,
    }

    public struct ExitGame
    {
        public ExitCode code;
        public Entrance.Link link;
    }
}
