using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Adapter
{
    public class UISummonResult : MonoBehaviour
    {
        private class OpenDirection
        {
            private readonly GameObject _defaultObj;
            private readonly ParticleSystem _openEffect;
            private readonly UISummonSlot _slot;
            private readonly float _duration;

            public bool Done { get; private set; }

            public OpenDirection(UISummonSlot slot)
            {
                _slot = slot;

                var root = _slot.transform.parent;
                _defaultObj = root.Find("Soul").gameObject;
                _openEffect = root.Find("Effect").GetComponent<ParticleSystem>();

                _duration = _openEffect.main.duration;

                _defaultObj.SetActive(true);
                _openEffect.gameObject.SetActive(false);
                _slot.gameObject.SetActive(false);
            }

            public async UniTaskVoid Run()
            {
                if (!Done)
                    return;

                Done = false;
                _openEffect.gameObject.SetActive(true);
                _openEffect.Play();

                await UniTask.Delay(TimeSpan.FromSeconds(_duration));

                _defaultObj.SetActive(false);
                _openEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _slot.gameObject.SetActive(true);
            }
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private UISummonSlot[] _slots;
        [SerializeField] private Button _showAll;
        [SerializeField] private Button _close;

        private readonly Dictionary<UISummonSlot, OpenDirection> _directions = new();

        public UniTask DisplayTask => _completionSource.Task;

        private UniTaskCompletionSource _completionSource;

        private bool _initialized;
        
        private void Initialize()
        {
            if (_initialized)
                return;

            this.SubscribeButton(_close, Close);
            this.SubscribeButton(_showAll, OpenAllSlots);
            _initialized = true;
        }

        public void Open(Summon data)
        {
            Initialize();

            _close.enabled = false;
            gameObject.SetActive(true);
            
            _animator.Play("Entrance");
            _directions.Clear();
            _completionSource = new();

            using var itr = data.list.GetEnumerator();
            int index = 0;
            while (index.IsValidIndex(_slots))
            {
                var slot = _slots[index];
                if (itr.MoveNext())
                {
                    slot.Init(itr.Current).Forget();
                    
                    var button = slot.GetComponentInParent<Button>();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OpenSlot(slot));

                    _directions[slot] = new OpenDirection(slot);
                }

                ++index;
            }
        }

        private void OpenSlot(UISummonSlot slot)
        {
            if (!_directions.TryGetValue(slot, out var direction))
                return;

            direction.Run().Forget();
            
            SetEndIfAllDone();
        }

        private void OpenAllSlots()
        {
            using var itr = _directions.Values.GetEnumerator();
            while (itr.MoveNext())
            {
                itr.Current?.Run().Forget();
            }

            _completionSource.TrySetResult();

            _close.enabled = true;
        }

        private void Close()
        {
            _completionSource.TrySetResult();
            
            gameObject.SetActive(false);
        }

        private void SetEndIfAllDone()
        {
            if (!IsEnd())
                return;

            _close.enabled = true;
        }

        private bool IsEnd()
        {
            bool isEnd = true;
            using var itr = _directions.Values.GetEnumerator();
            while (itr.MoveNext() && isEnd)
            {
                if (itr.Current is { Done: false })
                    isEnd = false;
            }

            return isEnd;
        }
    }
}