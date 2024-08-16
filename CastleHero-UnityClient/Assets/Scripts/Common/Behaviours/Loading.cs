using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Time = UnityEngine.Time;

namespace RGLabs.Common.Behaviours
{
    public class Loading : MonoBehaviour
    {
        public static List<UniTask> Tasks = new();
        
        public static string NextScene
        {
            set
            {
                _nextScene = value;
                SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
            }
        }
        
        private static string _nextScene;

        private void Start()
        {
            StartCoroutine(LoadSceneCoroutine(_nextScene));
        }
        
        private IEnumerator LoadSceneCoroutine(string load)
        {
            Context.Back.enabled = false;

            var loadHandle = SceneManager.LoadSceneAsync(load);
            while (!loadHandle.isDone)
                yield return null;

            if (Tasks.Count > 0)
            {
                yield return UniTask.WhenAll(Tasks).ToCoroutine();
            
                Tasks.Clear();                
            }
            
            var finishHandle = SceneManager.UnloadSceneAsync("Loading");
            while (!finishHandle.isDone)
                 yield return null;
            
            var scene = SceneManager.GetSceneByName(load);
            SceneManager.SetActiveScene(scene);
            Context.Back.enabled = true;
        }
    }
}