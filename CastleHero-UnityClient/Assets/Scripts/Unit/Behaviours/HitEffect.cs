using System.Collections;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class HitEffect : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _color = Color.white;

        private float _halfDuration;
        private int _propertyId;
        private MaterialPropertyBlock _propertyBlock;
        
        private void Start()
        {
            _halfDuration = Constants.HitEffectDuration * 0.5f;
            _propertyBlock = new MaterialPropertyBlock();
            _renderer.SetPropertyBlock(_propertyBlock);

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
            yield return StartCoroutine(HalfCycle(Color.black, _color));
            yield return StartCoroutine(HalfCycle(_color, Color.black));
        }
        
        private IEnumerator HalfCycle(Color start, Color end)
        {
            float currentTime = 0f;
            while (currentTime < _halfDuration)
            {
                currentTime += Time.deltaTime;
                ApplyColor(Color.Lerp(start, end, currentTime / _halfDuration));

                yield return UniTask.Yield();
            }
        }

        private void ApplyColor(Color color)
        {
            _propertyBlock.SetColor(_propertyId, color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}