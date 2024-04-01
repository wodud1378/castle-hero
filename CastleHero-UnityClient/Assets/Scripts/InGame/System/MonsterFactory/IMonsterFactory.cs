using System;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Data.Model;

namespace RGLabs.InGame.System.Spawn
{
    public interface IMonsterFactory
    {
        public void PushCreationRequest(MonsterEntity entity, Action<GameUnit> onCreated);
    }
}