using System;
using System.Collections.Generic;
using RGLabs.Common.Pattern;
using RGLabs.Data.Repositories;
using RGLabs.Unit.Factory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGLabs.Common.Behaviours
{
    public abstract class SceneBehaviour : MonoBehaviour, IDisposable
    {
        protected static List<SceneBehaviour> activated = new();

        protected DBCollections db;

        protected InGameRepository gameRepo;
        protected UserRepository userRepo;

        protected IUnitFactory unitFactory;
        protected PoolContainer poolContainer;

        public virtual void Dispose()
        {
            gameRepo.Dispose();
            poolContainer.Dispose();
        }

        protected void LoadSceneAfterDispose(string sceneName)
        {
            foreach (var behaviour in activated)
                behaviour.Dispose();
            
            activated.Clear();
            
            var current = SceneManager.GetActiveScene();
            Loading.prevScene = current;
            Loading.nextScene = sceneName;
            
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
        }
    }
}