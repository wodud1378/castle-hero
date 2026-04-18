using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CastleHero.View.Bootstrapper
{
    public abstract class SceneBase : MonoBehaviour, IDisposable
    {
        protected static List<SceneBase> activated = new();

        private void Awake()
        {
            Bootstrapper.OnLoadCompleteQueue.Enqueue(() => OnLoaded());

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
            foreach (var scene in activated)
                scene.Dispose();

            activated.Clear();

            Loading.NextScene = sceneName;
        }
    }
}