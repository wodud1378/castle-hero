using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public enum ExitCode
    {
        Exit,
        Retry,
        Next,
    }

    public struct ExitGame
    {
        public ExitCode code;
        public Entrance.Link link;
    }

    public struct GameResult
    {
        public bool isCleared;
        public int id;
        public GameType type;
        public GameCleared data;
    }

    public class InGameBehaviour : SceneBehaviour
    {
        [SerializeField] private UIInGame _uiInGame;
        [SerializeField] private WaveRunner _waveRunner;

        private GameHandler _handler;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeMessage<ExitGame>(Exit);
            this.SubscribeMessage<StartGame>(Run);
        }

        private void Run(StartGame startGame)
        {
            Time.timeScale = Storage.inGameRepository.speedUp.Value
                ? 2f
                : 1f;

            var cam = Camera.main;
            cam.DOOrthoSize(20f, 0.4f);

            _uiInGame.gameObject.SetActive(true);
            _uiInGame.Init();
            _uiInGame.Open();

            Context.startButton.enabled = false;

            _handler = new GameHandler(startGame, _waveRunner);
            _handler.OnFinished += OnFinished;

            InitGlobalSkills();

            _handler.OnStart();
        }

        private async void OnFinished(GameFinished finished)
        {
            finished.Publish();
            
            var isCleared = finished.isCleared;
            var type = finished.type;
            var id = finished.id;
            GameCleared data = null;
            if (isCleared)
            {
                var result = await NetworkService.Game.Clear(type, id);
                if (!result.IsSuccess)
                {
                    var param = new PopupCommon.ButtonParam
                    {
                        action = PopupCommon.ButtonAction.Confirm,
                        onClick = () =>
                        {
                            Exit(new ExitGame
                            {
                                code = ExitCode.Exit,
                                link = Entrance.Link.None
                            });
                        }
                    };

                    Context.popups.Open<PopupCommon>(result.error, param);
                    return;
                }

                data = result.data;
            }

            new GameResult
            {
                isCleared = isCleared,
                id = id,
                type = type,
                data = data
            }.Publish();

            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }

        private void InitGlobalSkills()
        {
            var ui = _uiInGame.GlobalSkill;
            if (Storage.inGameRepository.castle.Value != null)
            {
                ui.gameObject.SetActive(true);
                ui.Init(_handler.GetCastleSkills()).Forget();
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }

        private void Exit(ExitGame exit)
        {
            _handler.OnExit(exit);

            var task = NetworkService.User.GetUserData()
                .ContinueWith(x => Storage.userRepository.Update(x.data));

            Loading.Tasks.Add(task);
            LoadSceneAfterDispose("Main");
        }

        public override void Dispose()
        {
            base.Dispose();

            _uiInGame.Dispose();
            _handler.Dispose();
        }
    }
}