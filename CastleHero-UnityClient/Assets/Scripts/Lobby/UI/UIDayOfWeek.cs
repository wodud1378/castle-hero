using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RGLabs.Lobby.UI
{
    public class UIDayOfWeek : UISlot
    {
        [Serializable]
        public struct Preset
        {
            public string spritePath;
            public Color32 textColor;
        }

        [Serializable]
        public struct PresetWrap
        {
            public DayOfWeek dayOfWeek;
            public Preset preset;
        }

        [SerializeField] private Preset _forAll;
        [SerializeField] private List<PresetWrap> _presets;

        public readonly ReactiveCollection<DayOfWeek> values = new();

        private AsyncOperationHandle<Sprite> _bgHandle;

        private void Awake()
        {
            values
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(OnChanged)
                .AddTo(this);
        }

        private void OnChanged(ReactiveCollection<DayOfWeek> values)
        {
            if (values.Count == 0)
                return;

            Preset preset;
            string text;
            if (values.Count >= 7)
            {
                preset = _forAll;
                text = "All";
            }
            else
            {
                var dayOfWeek = values[0];

                preset = _presets.First(x => x.dayOfWeek == dayOfWeek).preset;
                text = Storage.localize.Get(dayOfWeek == DayOfWeek.Sunday
                    ? 570
                    : 563 + (int)dayOfWeek);
            }

            Init(preset.spritePath, text.WithColor(preset.textColor))
                .Forget();
        }
    }
}