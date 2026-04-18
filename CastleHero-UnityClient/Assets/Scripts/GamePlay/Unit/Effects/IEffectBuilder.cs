using CastleHero.GamePlay.Unit.Behaviours;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Effects
{
    /// <summary>
    /// 이펙트 빌더 체인 API. 구현체는 EffectBuilder (GamePlay).
    /// </summary>
    public interface IEffectBuilder
    {
        IEffectBuilder StartBuild(string prefab);
        IEffectBuilder From(Vector2 position);
        IEffectBuilder To(UnitActor unit);
        IEffectBuilder To(Vector2 position);
        IEffectBuilder LookAt(Vector2 forward);
        IEffectBuilder Duration(float value);
        void Run(string prefab, Vector2 to);
        IEffect Run();
    }
}
