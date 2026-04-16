using System.Collections.Generic;

namespace CastleHero.Data.Model
{
    public interface IEntity
    {
        public int Id { get; set; }
        public bool IsValid { get; set; }
    }

    public enum GameType
    {
        Stage,
        Dungeon,
    }

    public interface IGameEntity : IEntity
    {
        public int Lv { get; }
        public GameType Type { get; }
        public string Map { get; set; }
        public string Bgm { get; set; }
        public int Ap { get; set; }
        public int TimeLimit { get; set; }
        public int WaveId { get; set; }
        
        public int Exp { get; }
        public int MinGold { get; set; }
        public int MaxGold { get; set; }

        public List<Reward> GetRewardsForDisplay();
    }
}