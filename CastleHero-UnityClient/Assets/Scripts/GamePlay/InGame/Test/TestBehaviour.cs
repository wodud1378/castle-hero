using System;
using CastleHero.Data;
using CastleHero.Data.Load;
using CastleHero.GamePlay.InGame.System;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using CastleHero.GamePlay.InGame;

namespace CastleHero.GamePlay.InGame.Test
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            for (int i = 0; i < 4; ++i)
            {
                int index = i;
                SetKeyAction(KeyCode.Keypad0 + i, () => KillCharacter(index));
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
                KillCharacter(0);
            
            if(Input.GetKeyDown(KeyCode.Alpha2))
                KillCharacter(1);
            
            if(Input.GetKeyDown(KeyCode.Alpha3))
                KillCharacter(2);
            
            if(Input.GetKeyDown(KeyCode.Alpha4))
                KillCharacter(3);
        }

        private void SetKeyAction(KeyCode keyCode, Action action)
        {
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(keyCode))
                .Subscribe(_ => action.Invoke())
                .AddTo(this);
        }

        private void KillCharacter(int index)
        {
            var characters = InGameSession.Current.Characters;
            if (!index.IsValidIndex(characters))
                return;

            var behaviour = characters[index] as UnitBehaviour;
            if (behaviour == null)
                return;

            new AtkEvent
            {
                Type = DamageType.Normal,
                From = null,
                To = behaviour,
                Amount = behaviour.Status.hp.Max
            }.Publish();
        }
    }
}