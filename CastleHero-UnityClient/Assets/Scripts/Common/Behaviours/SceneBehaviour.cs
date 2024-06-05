using System;
using System.Collections.Generic;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Unit.Factory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGLabs.Common.Behaviours
{
    public abstract class SceneBehaviour : MonoBehaviour, IDisposable
    {
        protected static List<SceneBehaviour> activated = new();

        private void Awake()
        {
            Context.OnLoadCompleteQueue.Enqueue(OnLoaded);
            
            OnAwake();
        }

        protected virtual void OnLoaded()
        {
        }

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
            
            Storage.inGameRepository.Dispose();

            Loading.NextScene = sceneName;
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
        }
    }
}