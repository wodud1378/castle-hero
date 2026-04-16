using CastleHero.Common.Flow;
using CastleHero.Data.Repositories;

namespace CastleHero.Data.Model
{
    public struct Entrance
    {
        public enum Link
        {
            None,
            LevelUp,
            Equipment,
            RateUp,
            Dungeon,
        }

        public State state;
        public Link link;
        public GameEntrance gameEntrance;
    }
}
