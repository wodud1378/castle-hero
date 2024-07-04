using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Time = UnityEngine.Time;

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
                SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
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
            Context.Back.enabled = false;

            var loadHandle = SceneManager.LoadSceneAsync(load);
            while (!loadHandle.isDone)
                yield return null;
            
            var finishHandle = SceneManager.UnloadSceneAsync("Loading");
            while (!finishHandle.isDone)
                 yield return null;
            
            var scene = SceneManager.GetSceneByName(load);
            SceneManager.SetActiveScene(scene);
            Context.Back.enabled = true;
        }
    }
}