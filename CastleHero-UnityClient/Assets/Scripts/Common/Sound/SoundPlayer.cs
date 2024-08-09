using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RGLabs.Common.Sound
{
    public class SoundPlayer : IDisposable
    {
        private enum LoadState
        {
            None,
            InProgress,
            Done,
        }
        
        public bool IsPlaying => _source.isPlaying;
        
        public float NormalizedTime => _source.clip == null
                ? 1f
                : _source.time / _source.clip.length;

        public float Volume
        {
            set => _source.volume = value;
        }

        public readonly ReactiveProperty<string> asset = new();

        private readonly AudioSource _source;
        private readonly IDisposable _subscription;

        private CancellationTokenSource _ctSource;
        private AsyncOperationHandle<AudioClip> _handle;

        private LoadState _loadState;

        private UniTask<(bool IsCanceled, AudioClip Result)> _loadTask;
        private bool _loadDone;

        public SoundPlayer(AudioSource source)
        {
            _source = source;
            _subscription = asset.Subscribe(OnAssetChanged);
        }

        public void Play()
        {
            UniTask
                .RunOnThreadPool(()=> PlayInternal(asset.Value), cancellationToken: _ctSource.Token)
                .SuppressCancellationThrow();
        }

        public void Stop()
        {
            _source.Stop();
            _ctSource?.Cancel();

            _ctSource = new();
        }

        private void OnAssetChanged(string asset)
        {
            if (string.IsNullOrEmpty(asset))
                return;

            Stop();
            Play();
        }

        private async UniTask PlayInternal(string asset)
        {
            if (this.asset.Value != asset) 
                _loadState = LoadState.None;

            if (_loadState != LoadState.Done)
            {
                if (_loadState != LoadState.InProgress)
                {
                    if (_handle.IsValid())
                        _handle.Release();

                    _handle = Addressables.LoadAssetAsync<AudioClip>(asset);

                    _loadTask = _handle
                        .ToUniTask(cancellationToken: _ctSource.Token)
                        .SuppressCancellationThrow();

                    _loadState = LoadState.InProgress;
                }

                var result = await _loadTask;
                if (result.IsCanceled)
                {
                    _loadState = LoadState.None;
                    return;
                }

                _source.clip = result.Result;
                _loadState = LoadState.Done;
            }

            _source.Play();
        }

        public void Dispose()
        {
            asset?.Dispose();
            _subscription?.Dispose();
            _ctSource?.Dispose();
            
            _handle.Release();
        }
    }
}