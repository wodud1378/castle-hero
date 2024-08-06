using System;
using System.Collections.Generic;
using LitJson;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Common.Localize
{
    public class LocalizeText : IDisposable
    {
        public event Action OnLoaded; 
        
        private readonly Dictionary<int, string> _texts = new();
        private readonly JsonData _raw;
        private readonly IDisposable _subscription;
        
        public LocalizeText(JsonData raw) => _raw = raw;

        public string Get(int id) => _texts.TryGetValue(id, out var value) ? value : string.Empty; 

        public void Set(SystemLanguage language)
        {
            _texts.Clear();
            
            var key = LocalizeHelper.SystemLanguageToIso(language);
            foreach (JsonData data in _raw)
            {
                int id = data["String_ID"].ToInt();
                if (_texts.ContainsKey(id))
                    continue;
                
                _texts.Add(id, data[key].ToString());
            }
            
            OnLoaded?.Invoke();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}