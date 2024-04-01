using UnityEngine;

namespace RGLabs.InGame.Units.Interfaces
{
    public interface IMovement
    {
        public void Move(Vector2 direction, float speed, ref Vector2 moveVector);
    }
}
