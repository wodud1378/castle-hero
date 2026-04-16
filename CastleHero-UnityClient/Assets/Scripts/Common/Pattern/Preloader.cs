using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CastleHero.Common.Pattern
{
    public readonly struct PreloadEntry
    {
        public readonly string Path;
        public readonly int Count;

        public PreloadEntry(string path, int count)
        {
            Path = path;
            Count = count;
        }
    }

    /// <summary>
    /// 엔트리 목록을 받아 프리팹을 비동기 로드한 뒤 PoolContainer 에 Register.
    /// 프리로드 완료 후 반환되므로 이후 게임 코드는 모두 동기로 풀에서 가져올 수 있음.
    /// </summary>
    public class Preloader
    {
        public async UniTask PreloadAll(
            IEnumerable<PreloadEntry> entries,
            PoolContainer container,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            var list = new List<PreloadEntry>(entries);
            int total = list.Count;
            int done = 0;

            foreach (var entry in list)
            {
                if (ct.IsCancellationRequested) return;

                if (string.IsNullOrEmpty(entry.Path) || entry.Count <= 0)
                {
                    done++;
                    progress?.Report(total > 0 ? (float)done / total : 1f);
                    continue;
                }

                var handle = Addressables.LoadAssetAsync<GameObject>(entry.Path);
                await handle.ToUniTask(cancellationToken: ct).SuppressCancellationThrow();

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                    container.RegisterWithHandle(entry.Path, handle.Result, handle, entry.Count);

                done++;
                progress?.Report(total > 0 ? (float)done / total : 1f);
            }
        }
    }
}
