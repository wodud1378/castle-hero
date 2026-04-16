using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitJson;
using CastleHero.Utility;
using UnityEngine;

namespace CastleHero.Common.Localize
{
    public class LocalizeText
    {
        private struct Range
        {
            public int start;
            public int end;
        }

        public event Action OnLoaded;

        private readonly Dictionary<Range, Dictionary<int, string>> _map = new();
        private readonly JsonData _raw;

        private const string IdKey = "String_ID";

        public LocalizeText(JsonData raw) => _raw = raw;

        public string Get(int id)
        {
            string text = string.Empty;
            foreach (var kvp in _map)
            {
                var range = kvp.Key;
                if (id < range.start || id > range.end)
                    continue;

                if (!kvp.Value.TryGetValue(id, out text))
                    text = string.Empty;
            }

            return string.IsNullOrEmpty(text)
                ? $"[{id}] id에 해당하는 텍스트가 없습니다.".WithNegativeColor()
                : text;
        }

        public async UniTask Set(SystemLanguage language)
        {
            var iso = LocalizeHelper.SystemLanguageToIso(language);

            await ProcessDataAsync(iso, 100);

            OnLoaded?.Invoke();
        }

        private async UniTask ProcessDataAsync(string iso, int chunkSize)
        {
            _map.Clear();

            int totalChunks = (_raw.Count + chunkSize - 1) / chunkSize;
            var tasks = new List<UniTask<KeyValuePair<Range, Dictionary<int, string>>>>();

            for (int i = 0; i < totalChunks; i++)
            {
                int startIndex = i * chunkSize;
                tasks.Add(UniTask.RunOnThreadPool(() => ProcessChunk(iso, startIndex, chunkSize)));
            }

            var resultChunks = await UniTask.WhenAll(tasks);

            foreach (var chunk in resultChunks)
            {
                _map.Add(chunk.Key, chunk.Value);
            }
        }

        private KeyValuePair<Range, Dictionary<int, string>> ProcessChunk(string iso, int startIndex, int chunkSize)
        {
            int endIndex = Mathf.Min(startIndex + chunkSize, _raw.Count) - 1;
            var chunk = new KeyValuePair<Range, Dictionary<int, string>>(
                new Range { start = int.Parse(_raw[startIndex][IdKey].ToString()), end = int.Parse(_raw[endIndex][IdKey].ToString()) },
                new Dictionary<int, string>());

            for (int i = startIndex; i <= endIndex; ++i)
            {
                var item = _raw[i];
                int id = int.Parse(item[IdKey].ToString());
                string text = item[iso].ToString();

                chunk.Value[id] = text;
            }
            return chunk;
        }
    }
}
