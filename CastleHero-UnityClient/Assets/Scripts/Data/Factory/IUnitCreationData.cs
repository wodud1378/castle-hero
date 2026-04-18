using CastleHero.Network.Shared;
using UnityEngine;

namespace CastleHero.Data.Factory
{
    public interface IUnitCreationData
    {
        UnitInfo Info { get; }
        Vector2 Position { get; }
    }

    public readonly struct UnitCreationData : IUnitCreationData
    {
        public UnitInfo Info { get; }
        public Vector2 Position { get; }

        public UnitCreationData(UnitInfo info, Vector2 position)
        {
            Info = info;
            Position = position;
        }
    }

    public readonly struct CastleCreationData : IUnitCreationData
    {
        public string Prefab { get; }
        public UnitInfo Info { get; }
        public Vector2 Position { get; }

        public CastleCreationData(UnitInfo info, Vector2 position, string prefab = null)
        {
            Info = info;
            Position = position;
            Prefab = prefab;
        }
    }

    public readonly struct BarricadeCreationData : IUnitCreationData
    {
        public UnitInfo Info { get; }
        public Vector2 Position { get; }

        public BarricadeCreationData(UnitInfo info, Vector2 position)
        {
            Info = info;
            Position = position;
        }
    }
}
