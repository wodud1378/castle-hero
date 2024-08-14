using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Common.Localize
{
    public class LocalizeText
    {
        public event Action OnLoaded;

        private readonly Dictionary<int, Dictionary<int, string>> _map = new();
        private readonly JsonData _raw;

        public LocalizeText(JsonData raw) => _raw = raw;

        public string Get(int id)
        {
            int division = id / 100;

            return _map.TryGetValue(division, out var texts) &&
                   texts.TryGetValue(id, out var text)
                ? text
                : $"[{id}] id에 해당하는 텍스트가 없습니다.".WithNegativeColor();
        }

        public async UniTask Set(SystemLanguage language)
        {
            int length = _raw.Count;
            int division = length / 100;
            var key = LocalizeHelper.SystemLanguageToIso(language);
            for (int i = 0; i <= division; ++i)
            {
                if (!_map.TryGetValue(i, out var texts))
                {
                    texts = new();
                    _map[i] = texts;
                }
                else
                    texts.Clear();

                await UniTask.RunOnThreadPool(() =>
                {
                    int start = i * 100;
                    int max = Mathf.Min(start + 100, length - division * i);
                    for (int j = start; j < max; ++j)
                    {
                        texts.Add(j, _raw[j][key].ToString());
                    }
                });
            }

            OnLoaded?.Invoke();
        }
    }
}