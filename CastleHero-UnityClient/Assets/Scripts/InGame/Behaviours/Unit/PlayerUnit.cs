using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit
{
    public class PlayerUnit : GameUnit
    {
        protected override Vector2 Destination() => Position;
    }
}