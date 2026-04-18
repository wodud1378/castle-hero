using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/PopupSummonDirection.prefab")]
    public class PopupSummonDirection : PopupBase
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
                if (Done)
                    return;

                Done = true;
                _defaultObj.SetActive(false);
                _slot.gameObject.SetActive(true);
                _openEffect.gameObject.SetActive(true);
                _openEffect.Play();

                await UniTask.Delay(TimeSpan.FromSeconds(_duration));

                _openEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        [FormerlySerializedAs("_slots")]
        [SerializeField] private UISummonSlot[] slots;
        [FormerlySerializedAs("_showAll")]
        [SerializeField] private Button showAll;

        private readonly Dictionary<UISummonSlot, OpenDirection> _directions = new();

        public UniTask DisplayTask => _completionSource.Task;

        private UniTaskCompletionSource _completionSource;

        private bool _initialized;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(showAll, OpenAllSlots);
        }

        public override UniTask Open(params object[] parameters)
        {
            _close.gameObject.SetActive(false);

            var data = (Summon)parameters[0];
            _completionSource = new();

            using var itr = data.list.GetEnumerator();
            int index = 0;
            while (index.IsValidIndex(slots))
            {
                var slot = slots[index];
                if (itr.MoveNext())
                {
                    slot.Init(itr.Current);
                    var button = slot.GetComponentInParent<Button>();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OpenSlot(slot));

                    _directions[slot] = new OpenDirection(slot);
                }

                ++index;
            }

            return UniTask.CompletedTask;
        }

        private void OpenSlot(UISummonSlot slot)
        {
            if (!_directions.TryGetValue(slot, out var direction))
                return;

            direction.Run().SafeForget();

            SetEndIfAllDone();
        }

        private void OpenAllSlots()
        {
            using var itr = _directions.Values.GetEnumerator();
            while (itr.MoveNext())
            {
                itr.Current?.Run().SafeForget();
            }

            _completionSource.TrySetResult();

            showAll.gameObject.SetActive(false);
            _close.gameObject.SetActive(true);
        }

        protected override void OnClose()
        {
            base.OnClose();

            _completionSource.TrySetResult();
        }

        private void SetEndIfAllDone()
        {
            if (!IsEnd())
                return;

            showAll.gameObject.SetActive(false);
            _close.gameObject.SetActive(true);
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
