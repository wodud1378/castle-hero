using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.Data;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.GamePlay.InGame;
using CastleHero.Data.Model;

namespace CastleHero.View.InGame.UI
{
    public class UIPause : MonoBehaviour, IBackButtonListener
    {
        [FormerlySerializedAs("_exitButton")]
        [SerializeField] private Button exitButton;
        [FormerlySerializedAs("_resumeButton")]
        [SerializeField] private Button resumeButton;
        [FormerlySerializedAs("_retryButton")]
        [SerializeField] private Button retryButton;

        private void Awake()
        {
            this.SubscribeButton(exitButton, Exit);
            this.SubscribeButton(resumeButton, Close);
            this.SubscribeButton(retryButton, Retry);
        }

        public void Open()
        {
            Time.timeScale = 0f;

            gameObject.SetActive(true);

            ServiceLocator.Get<CastleHero.Common.Flow.BackButton>().Add(this);
        }

        public void Close()
        {
            Time.timeScale = ServiceLocator.Get<CastleHero.Data.Repositories.SettingRepository>().speedUp.Value ? 2f : 1f;

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
