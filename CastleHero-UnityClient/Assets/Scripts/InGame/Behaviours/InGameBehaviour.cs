using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
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
    
    public struct GameResult
    {
        public bool isCleared;
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
            
            this.SubscribeMessage<ExitCode>(Exit);
            this.SubscribeMessage<StartGame>(Run);
        }

        private void Run(StartGame startGame)
        {
            Time.timeScale = Storage.inGameRepository.speedUp
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
            var data = finished.isCleared
                ? await NetworkService.Game.Clear(finished.type, finished.id)
                : null;
            
            new GameResult
            {
                isCleared = finished.isCleared,
                data = data
            }.Publish();
        }

        private void InitGlobalSkills()
        {
            _uiInGame.GlobalSkill
                .Init(_handler.GetCastleSkills())
                .Forget();
        }

        private void Exit(ExitCode exitCode)
        {
            _handler.OnExit(exitCode);
            
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