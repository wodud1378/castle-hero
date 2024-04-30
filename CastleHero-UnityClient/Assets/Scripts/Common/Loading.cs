using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGLabs.Common
{
    public class Loading : MonoBehaviour
    {
        public static Scene prevScene;
        public static string nextScene;

        private void Start()
        {
            StartCoroutine(LoadSceneCoroutine(prevScene, nextScene));
        }

        private IEnumerator LoadSceneCoroutine(Scene unload, string load)
        {
            var unloadHandle = SceneManager.UnloadSceneAsync(unload);
            yield return UniTask.WaitUntil(() => unloadHandle.isDone);

            var loadHandle = SceneManager.LoadSceneAsync(load, LoadSceneMode.Additive);
            
            yield return UniTask.WaitUntil(() => loadHandle.isDone);
            yield return null; 

            var scene = SceneManager.GetSceneByName(load);
            SceneManager.SetActiveScene(scene);
            
            SceneManager.UnloadSceneAsync("Loading");
        }
    }
}