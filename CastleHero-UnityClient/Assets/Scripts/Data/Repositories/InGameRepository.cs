using System;
using System.Linq;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Network.Shared;
using RGLabs.Unit.Behaviours;
using UniRx;

namespace RGLabs.Data.Repositories
{
    public class InGameRepository : IDisposable
    {
        public IGameEntity gameEntity;
        public bool speedUp;
        
        public readonly ReactiveProperty<int> mana = new(0);
        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveCollection<UnitBehaviour> characters = new();
        public readonly ReactiveCollection<UnitBehaviour> deadCharacters = new();
        public readonly ReactiveCollection<WaitRecover> recovers = new();
        public readonly ReactiveProperty<float> leftTime = new();
        
        public void Dispose()
        {
            castle.Value = null;
            characters?.Dispose();
            characters?.Clear();
            
            recovers?.Dispose();
            recovers?.Clear();

            castle?.Dispose();
            characters?.Dispose();
        }
    }
}