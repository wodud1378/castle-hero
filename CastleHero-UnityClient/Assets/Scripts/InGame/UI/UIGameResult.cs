using RGLabs.InGame.Behaviours;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIGameResult : MonoBehaviour
    {
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _nextButton;

        [SerializeField] private GameObject[] _clearObjects;
        [SerializeField] private GameObject[] _failedObjects;
        
        private void Awake()
        {
            this.SubscribeButton(_exitButton, Exit);
            this.SubscribeButton(_retryButton, Retry);
            this.SubscribeButton(_nextButton, Next);
        }

        public void Open(bool isCleared)
        {
            UpdateUI(isCleared);
            
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void UpdateUI(bool isCleared)
        {
            foreach (var obj in _clearObjects)
                obj.SetActive(isCleared);
            
            foreach (var obj in _failedObjects)
                obj.SetActive(!isCleared);
        }

        private void Exit() => ExitCode.Exit.Publish();

        private void Retry() => ExitCode.Retry.Publish();

        private void Next() => ExitCode.Next.Publish();
    }
}