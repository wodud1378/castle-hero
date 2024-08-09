using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.Sound
{
    [RequireComponent(typeof(AudioListener))]
    public class SoundManager : MonoBehaviour, IDisposable
    {
        [SerializeField] private int _maxSfx = 10;
        
        private readonly List<SoundPlayer> _sfxList = new();

        private SoundPlayer _bgm;
        private SettingRepository _repository;

        private void Awake()
        {
            _repository = Storage.settingRepository;
            
            InitPlayers();
            InitSubscription();
        }

        private void OnDestroy() => Dispose();

        private void InitSubscription()
        {
            _repository.bgmLevel
                .Subscribe(x =>
                {
                    if (x > 0f)
                        _bgm.Volume = x;
                    else
                        _bgm.Stop();
                })
                .AddTo(this);
            
            _repository.fxLevel
                .Subscribe(x =>
                {
                    if(x > 0f)
                        _sfxList.ForEach(player => player.Volume = x);
                    else
                        _sfxList.ForEach(player => player.Stop());
                })
                .AddTo(this);

            _repository.bgmToggle
                .Subscribe(x =>
                {
                    if(x)
                        _bgm.Play();
                    else
                        _bgm.Stop();
                })
                .AddTo(this);

            _repository.fxToggle
                .Subscribe(x =>
                {
                    if (!x)
                        _sfxList.ForEach(player => player.Stop());
                })
                .AddTo(this);
        }
        
        private void InitPlayers()
        {
            _bgm = new (gameObject.AddComponent<AudioSource>());
            for (int i = 0; i < _maxSfx; ++i)
            {
                _sfxList.Add(new (gameObject.AddComponent<AudioSource>()));
            }
        }

        public void PlayBgm(string asset)
        {
            if (!_repository.bgmToggle.Value)
                return;
            
            _bgm.asset.Value = asset;
        }

        public void PlaySfx(string asset)
        {
            if (!_repository.fxToggle.Value)
                return;
            
            var player = _sfxList.FirstOrDefault(x => !x.IsPlaying) ?? _sfxList
                .OrderByDescending(x => x.NormalizedTime)
                .First();

            player.asset.Value = asset;
        }

        public void Dispose()
        {
            _bgm.Dispose();
            _sfxList.ForEach(x => x.Dispose());
        }
    }
}