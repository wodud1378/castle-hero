using System;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public class UIControl : MonoBehaviour
    {
        public enum Step
        {
            Lobby,
            Stage,
            InGame,
        }

        [Serializable]
        public struct TriggerSet
        {
            public Step step;
            public string trigger;
            public Animator animator;
        }

        [SerializeField] private TriggerSet[] _triggerSets;

        public readonly ReactiveProperty<Step> step = new(Step.Lobby);

        private void Awake()
        {
            step.DistinctUntilChanged()
                .Skip(1)
                .Subscribe(x =>
                {
                    foreach (var set in _triggerSets)
                    {
                        if (set.step != x)
                            continue;

                        set.animator.SetTrigger(set.trigger);
                    }
                });
        }
    }
}