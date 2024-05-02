using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGLabs.Common.Behaviours
{
    public class Loading : MonoBehaviour
    {
        public static string NextScene
        {
            set
            {
                _nextScene = value;
                _prevScene = SceneManager.GetActiveScene();
            }
        }
        
        private static Scene _prevScene;
        private static string _nextScene;

        private void Start()
        {
            StartCoroutine(LoadSceneCoroutine(_prevScene, _nextScene));
        }

        private IEnumerator LoadSceneCoroutine(Scene unload, string load)
        {
            var loadHandle = SceneManager.LoadSceneAsync(load, LoadSceneMode.Additive);
            yield return UniTask.WaitUntil(() => loadHandle.isDone);
            yield return UniTask.Yield();
            
            var unloadHandle = SceneManager.UnloadSceneAsync(unload);
            yield return UniTask.WaitUntil(() => unloadHandle.isDone);
            yield return UniTask.Yield();

            var scene = SceneManager.GetSceneByName(load);
            SceneManager.SetActiveScene(scene);
            
            SceneManager.UnloadSceneAsync("Loading");
        }
    }
}