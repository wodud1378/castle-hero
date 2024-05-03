using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIInGame : MonoBehaviour, IDisposable
    {
        [field:SerializeField] public UICharacterList CharacterList { get; private set; }
        [field:SerializeField] public UIGameResult Result { get; private set; }
        [field:SerializeField] public UIPause Pause { get; private set; }

        [SerializeField] private RectTransform _damageRoot;
        [SerializeField] private string _damagePrefab;
        
        [SerializeField] private Button _pause;
        
        private InGameRepository _repository;
        private DBCollections _db;
        private PoolContainer _poolContainer;
        
        private void Awake()
        {
            this.SubscribeButton(_pause, ()=> SetPause(true));
            
            MessageBroker.Default
                .Receive<Result>()
                .Subscribe(OnResult)
                .AddTo(this);
            
            MessageBroker.Default
                .Receive<AtkResult>()
                .Subscribe(OnAtkResult)
                .AddTo(this);
        }
        
        private async void OnAtkResult(AtkResult result)
        {
            var pool = _poolContainer.Get(_damagePrefab);
            var uiDamage = await pool.Get() as UIDamage;
            if (uiDamage == null)
                return;

            var tr = uiDamage.transform;
            tr.SetParent(_damageRoot);
            tr.localScale = Vector3.one;
            uiDamage.Container = _poolContainer;
            uiDamage.Show(result);
        }

        public void InitAsync(PoolContainer poolContainer)
        {
            _repository = Storage.inGameRepository;
            _poolContainer = poolContainer;
            _db = Storage.DB;

            // var characters = _repository.characters.Value
            //     .Select(x => x.character)
            //     .ToArray();
            //
            // await CharacterList.Init(characters, _db.characters);
        }

        private void SetPause(bool isPause)
        {
            if(isPause)
                Pause.Open();
            else
                Pause.Close();
        }

        // private void OnApplicationFocus(bool hasFocus)
        // {
        //     SetPause(!hasFocus);
        // }

        private void OnResult(Result result)
        {
            Result.Open(result.isCleared);
        }

        public void Dispose()
        {
            CharacterList.Dispose();
        }
    }
}