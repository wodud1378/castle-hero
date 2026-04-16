using UnityEngine;

namespace CastleHero.Common.Behaviours
{
    public interface IUnitBehaviour
    {
        int Id { get; }
        Vector2 Position { get; set; }
        void DestroySelf();
        void Recovery();
    }
}
