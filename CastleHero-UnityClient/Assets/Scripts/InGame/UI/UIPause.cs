using RGLabs.InGame.Behaviours;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIPause : MonoBehaviour
    {
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _retryButton;

        private void Awake()
        {
            this.SubscribeButton(_exitButton, Exit);
            this.SubscribeButton(_resumeButton, Close);
            this.SubscribeButton(_retryButton, Retry);
        }

        public void Open()
        {
            Time.timeScale = 0f;
            
            gameObject.SetActive(true);
        }

        public void Close()
        {
            Time.timeScale = 1f;
            
            gameObject.SetActive(false);
        }

        private void Exit()
        {
            ExitCode.Exit.Publish();
            
            Close();
        }

        private void Retry()
        {
            ExitCode.Retry.Publish();
            
            Close();
        }
    }
}