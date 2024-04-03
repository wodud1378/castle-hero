using System;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;

namespace RGLabs.InGame.System.Spawn
{
    public interface IUnitFactory
    {
        public void PushCreationRequest<T>(UnitEntity entity, Action<T> onCreated) where T : GameUnit;
    }
}