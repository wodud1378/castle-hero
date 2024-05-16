using System;
using RGLabs.Data;
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

        private void SetKeyAction(KeyCode keyCode, Action action)
        {
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(keyCode))
                .Subscribe(_ => action.Invoke())
                .AddTo(this);
        }

        private void KillCharacter(int index)
        {
            var characters = Storage.inGameRepository.characters.Value;
            if (!index.IsValidIndex(characters))
                return;

            var behaviour = characters[index].behaviour;
            new AtkResult
            {
                type = DamageType.Normal,
                from = null,
                to = behaviour,
                amount = behaviour.status.hp.Max,
                isCritical = false
            }.Publish();
        }
    }
}