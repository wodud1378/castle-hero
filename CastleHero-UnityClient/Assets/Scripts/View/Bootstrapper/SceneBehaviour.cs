using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CastleHero.View.Bootstrapper
{
    public abstract class SceneBehaviour : MonoBehaviour, IDisposable
    {
        protected static List<SceneBehaviour> activated = new();

        private void Awake()
        {
            Context.OnLoadCompleteQueue.Enqueue(() => OnLoaded());

            OnAwake();
        }

        protected virtual UniTask OnLoaded() => UniTask.CompletedTask;

        protected virtual void OnAwake()
        {
            activated.Add(this);
        }

        public virtual void Dispose()
        {
        }

        protected void LoadSceneAfterDispose(string sceneName)
        {
            foreach (var behaviour in activated)
                behaviour.Dispose();

            activated.Clear();

            Loading.NextScene = sceneName;
        }
    }
}