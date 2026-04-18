using UnityEngine;

namespace CastleHero.Common.Behaviours
{
    public interface IUnitActor
    {
        int Id { get; }
        Vector2 Position { get; set; }
        void DestroySelf();
        void Recovery();
    }
}
