using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Common.Localize;
namespace CastleHero.View.Lobby.UI
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

        [FormerlySerializedAs("_forAll")]
        [SerializeField] private Preset forAll;
        [FormerlySerializedAs("_presets")]
        [SerializeField] private List<PresetWrap> presets;

        public readonly ReactiveCollection<DayOfWeek> values = new();

        private AsyncOperationHandle<Sprite> _bgHandle;

        protected override void OnAwake()
        {
            base.OnAwake();

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
                preset = forAll;
                text = "All";
            }
            else
            {
                var dayOfWeek = values[0];

                preset = presets.First(x => x.dayOfWeek == dayOfWeek).preset;
                text = ServiceLocator.Get<LocalizeText>().Get(dayOfWeek == DayOfWeek.Sunday
                    ? 570
                    : 563 + (int)dayOfWeek);
            }

            Init(preset.spritePath, text.WithColor(preset.textColor))
                .Forget();
        }
    }
}
