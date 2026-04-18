using System;
using System.Collections.Generic;
using System.Linq;
using CastleHero.Common.Sound;
using CastleHero.Data;
using CastleHero.Data.Repositories;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
namespace CastleHero.View.Sound
{
    [RequireComponent(typeof(AudioListener))]
    public class SoundManager : MonoBehaviour, IDisposable, ISoundManager
    {
        [FormerlySerializedAs("_maxSfx")]
        [SerializeField] private int maxSfx = 20;

        private readonly List<SoundPlayer> _sfxList = new();

        private SoundPlayer _bgm;
        private ISettingRepository _repository;

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            _repository = sl.Get<ISettingRepository>();

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
            _bgm = CreatePlayer("BGM", true);

            for (int i = 0; i < maxSfx; ++i)
            {
                _sfxList.Add(CreatePlayer($"Sfx_{i + 1:D2}"));
            }
        }

        private SoundPlayer CreatePlayer(string name, bool loop = false)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            obj.transform.position = Vector3.zero;

            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;

            return obj.AddComponent<SoundPlayer>();
        }

        public void PlayBgm(string asset)
        {
            if (!_repository.bgmToggle.Value)
                return;

            _bgm.Play(asset);
        }

        public void StopBgm() => _bgm.Stop();

        public void PlaySfx(string asset)
        {
            if (!_repository.fxToggle.Value)
                return;

            var player = _sfxList.FirstOrDefault(x => !x.IsPlaying) ?? _sfxList
                .OrderByDescending(x => x.NormalizedTime)
                .First();

            player.Play(asset);
        }

        public void Dispose()
        {
            _bgm.Dispose();
            _sfxList.ForEach(x => x.Dispose());
        }
    }
}
