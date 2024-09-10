using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.Behaviours;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIPause : MonoBehaviour, IBackButtonListener
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
            
            Context.Back.Add(this);
        }

        public void Close()
        {
            Time.timeScale = Storage.inGameRepository.speedUp.Value ? 2f : 1f;
            
            gameObject.SetActive(false);
        }

        private void Exit()
        {
            new ExitGame
            {
                code = ExitCode.Exit,
                link = Entrance.Link.None
            }.Publish();

            Close();
        }

        private void Retry()
        {
            new ExitGame
            {
                code = ExitCode.Retry,
                link = Entrance.Link.None
            }.Publish();
            
            Close();
        }

        public bool OnProcessBack()
        {
            Close();
            
            return true;
        }
    }
}