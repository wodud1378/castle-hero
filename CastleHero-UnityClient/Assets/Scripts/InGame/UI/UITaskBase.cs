using System;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public abstract class UITaskBase : MonoBehaviour
    { 
        public enum State
        {
            None,
            Done,
        }

        public ReactiveProperty<State> result = new(State.None);

        private void OnEnable()
        {
            result.Value = State.None;
        }

        private void OnDisable()
        {
            result.Dispose();
        }
    }
}