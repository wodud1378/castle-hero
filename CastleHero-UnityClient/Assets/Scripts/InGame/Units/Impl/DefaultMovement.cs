using RGLabs.InGame.Units.Interfaces;
using UnityEngine;

namespace RGLabs.InGame.Units.Impl
{
    public class DefaultMovement : IMovement
    {
        public void Move(Vector2 direction, float speed, ref Vector2 moveVector) => moveVector = direction * speed;
    }
}