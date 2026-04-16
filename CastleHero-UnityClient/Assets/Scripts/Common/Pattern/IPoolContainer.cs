using System;
using CastleHero.Common.Behaviours;
using UnityEngine;

namespace CastleHero.Common.Pattern
{
    /// <summary>
    /// 어드레서블 기반 오브젝트 풀 컨테이너.
    /// 리팩토링 후: 모든 조회는 동기. 프리팹은 외부(Preloader) 가 로드 후 <see cref="Register"/> 로 주입.
    /// </summary>
    public interface IPoolContainer : IDisposable
    {
        /// <summary>프리로드된 프리팹으로 풀을 등록. preloadCount 만큼 미리 Instantiate.</summary>
        void Register(string path, GameObject prefab, int preloadCount = 0);

        bool TryGet<T>(string resourcePath, out T item, Vector2 position = default) where T : PoolItemBase;
        bool TryGet(string resourcePath, out PoolItemBase item, Vector2 position = default);

        void Release(PoolItemBase item);
    }
}
