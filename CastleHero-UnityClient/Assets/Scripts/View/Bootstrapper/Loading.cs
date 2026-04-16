using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Time = UnityEngine.Time;
using CastleHero.Common.Pattern;
using CastleHero.Common.Flow;

namespace CastleHero.View.Bootstrapper
{
    public class Loading : MonoBehaviour
    {
        /// <summary>
        /// 씬 전환 완료 전에 완료되어야 할 작업 큐. Loading 씬이 로드될 때 SNAPSHOT 을 떠서 처리 후 원본을 Clear 한다.
        /// 접근은 동일 프레임/단일 스레드 가정. 큐 추가는 이 static 리스트에 한다.
        /// </summary>
        public static readonly List<UniTask> Tasks = new();

        /// <summary>
        /// 새 씬의 Context.LoadAsync (Preloader 포함) 가 끝났음을 알리는 handshake.
        /// 전환 직전에 새로 생성되고, 새 씬이 로드를 완료하면 TrySetResult 로 시그널. Loading 코루틴이 이 태스크를 await.
        /// </summary>
        public static UniTaskCompletionSource SceneReady { get; private set; }

        public static string NextScene
        {
            set
            {
                _nextScene = value;
                SceneReady = new UniTaskCompletionSource();
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
            ServiceLocator.Get<BackButton>().enabled = false;

            // Loading 진입 시점에 현재 쌓인 Tasks 를 스냅샷 하고 원본은 즉시 Clear.
            // 이유: 로드 중 외부에서 다시 Enqueue 한 작업이 있다면 다음 Loading 씬에서 처리되어야 함.
            var snapshot = Tasks.Count > 0 ? new List<UniTask>(Tasks) : null;
            Tasks.Clear();

            var loadHandle = SceneManager.LoadSceneAsync(load);
            while (!loadHandle.isDone)
                yield return null;

            if (snapshot != null)
                yield return UniTask.WhenAll(snapshot).ToCoroutine();

            // 새 씬의 Context.LoadAsync (프리로드 포함) 가 끝날 때까지 대기.
            if (SceneReady != null)
                yield return SceneReady.Task.ToCoroutine();

            yield return null;

            var finishHandle = SceneManager.UnloadSceneAsync("Loading");
            while (!finishHandle.isDone)
                 yield return null;

            var scene = SceneManager.GetSceneByName(load);
            SceneManager.SetActiveScene(scene);
            ServiceLocator.Get<BackButton>().enabled = true;
        }
    }
}