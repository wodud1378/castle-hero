using System;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;

namespace RGLabs.InGame.System.UnitFactory
{
    public interface IUnitFactory
    {
        public void PushCreationRequest<T>(UnitEntity entity, Action<T> onCreated) where T : GameUnit;
    }
}