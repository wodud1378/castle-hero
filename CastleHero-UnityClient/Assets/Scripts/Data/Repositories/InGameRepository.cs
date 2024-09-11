using System;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public class InGameRepository : IDisposable
    {
        public IGameEntity gameEntity;
        public readonly BoolReactiveProperty speedUp;
        public readonly BoolReactiveProperty repeat = new(false);
        public readonly ReactiveProperty<int> mana = new(0);
        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveCollection<UnitBehaviour> characters = new();
        public readonly ReactiveCollection<UnitBehaviour> deadCharacters = new();
        public readonly ReactiveCollection<WaitRecover> recovers = new();
        public readonly ReactiveProperty<float> leftTime = new();

        public InGameRepository()
        {
            const string key = "speed-up";
            
            speedUp = new BoolReactiveProperty(PlayerPrefs.GetInt(key, 0) == 1);
            speedUp.Subscribe(x => PlayerPrefs.SetInt(key, x ? 1 : 0));
        }
        
        public void Dispose()
        {
            castle.Value = null;
            
            speedUp?.Dispose();
            characters?.Dispose();
            characters?.Clear();
            
            recovers?.Dispose();
            recovers?.Clear();

            castle?.Dispose();
            characters?.Dispose();
        }
    }
}