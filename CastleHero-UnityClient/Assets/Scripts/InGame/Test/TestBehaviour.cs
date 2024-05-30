using System;
using RGLabs.Data;
using RGLabs.Data.Load;
using RGLabs.InGame.System;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.InGame.Test
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
            var characters = Storage.inGameRepository.characters;
            if (!index.IsValidIndex(characters))
                return;

            var behaviour = characters[index];
            new AtkEvent
            {
                Type = DamageType.Normal,
                From = null,
                To = behaviour,
                Amount = behaviour.status.hp.Max
            }.Publish();
        }
    }
}