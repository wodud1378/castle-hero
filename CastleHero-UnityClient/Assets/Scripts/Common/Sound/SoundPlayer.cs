using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CastleHero.Common.Sound
{
    public class SoundPlayer : MonoBehaviour, IDisposable
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

        public bool Loop
        {
            get => _source.loop;
            set => _source.loop = value;
        }

        private string _asset;
        private AudioSource _source;

        private AsyncOperationHandle<AudioClip> _handle;
        private Coroutine _coroutine;

        private LoadState _loadState;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        public void Play(string asset)
        {
            if (string.IsNullOrEmpty(asset))
                return;
            
            Stop();
            StartCoroutine(PlayInternal(asset));
        }

        public void Play()
        {
            if (string.IsNullOrEmpty(_asset))
                return;
            
            Stop();
            StartCoroutine(PlayInternal(_asset));
        }

        public void Stop()
        {
            if(_coroutine != null)
                StopCoroutine(_coroutine);
            
            _source.Stop();
        }

        private IEnumerator PlayInternal(string asset)
        {
            if (_asset != asset)
                _loadState = LoadState.None;

            if (_loadState != LoadState.Done)
            {
                if (_loadState != LoadState.InProgress)
                {
                    _handle.Release();
                    _handle = Addressables.LoadAssetAsync<AudioClip>(asset);
                    
                    _loadState = LoadState.InProgress;
                }
                
                yield return new WaitUntil(() => _handle.IsDone);

                _source.clip = _handle.Result;
                _loadState = LoadState.Done;
            }
            
            _source.Play();
        }

        private void OnDestroy() => Dispose();

        public void Dispose()
        {
            _handle.Release();
        }
    }
}