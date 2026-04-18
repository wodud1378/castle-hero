using System.Collections;
using CastleHero.GamePlay.Unit.Behaviours;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common;
using CastleHero.Common.Pattern;
namespace CastleHero.View.Unit
{
    public class HitEffect : MonoBehaviour, IHitEffect
    {
        [FormerlySerializedAs("_renderer")]
        [SerializeField] private Renderer renderer;
        [FormerlySerializedAs("_color")]
        [SerializeField] private Color color = Color.white;

        private GameConstants _constants;
        private float _halfDuration;
        private int _propertyId;
        private MaterialPropertyBlock _propertyBlock;
        private YieldAwaitable _wait = UniTask.Yield();

        private void Awake()
        {
            _constants = ServiceLocator.Instance.Get<GameConstants>();
        }

        public void Init()
        {
            _halfDuration = _constants.hitEffectDuration * 0.5f;
            _propertyBlock = new MaterialPropertyBlock();
            renderer.SetPropertyBlock(_propertyBlock);

            _propertyId = Shader.PropertyToID("_Black");

            ApplyColor(Color.black);
        }

        public void Play()
        {
            StopCoroutine(ColorChange());
            StartCoroutine(ColorChange());
        }

        private IEnumerator ColorChange()
        {
            yield return StartCoroutine(HalfCycle(Color.black, color));
            yield return StartCoroutine(HalfCycle(color, Color.black));
        }

        private IEnumerator HalfCycle(Color start, Color end)
        {
            float currentTime = 0f;
            while (currentTime < _halfDuration)
            {
                currentTime += Time.deltaTime;
                ApplyColor(Color.Lerp(start, end, currentTime / _halfDuration));

                yield return _wait;
            }
        }

        private void ApplyColor(Color color)
        {
            _propertyBlock.SetColor(_propertyId, color);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
