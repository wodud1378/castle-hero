using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Pattern;
using RGLabs.Data.DB;
using RGLabs.Data.Repositories;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    [Serializable]
    public struct DamagePrefabs
    {
        public string normal;
    }
    
    public class UIInGame : UIMain
    {
        [field:SerializeField] public UICharacterList DeadCharacters { get; private set; }
        [field:SerializeField] public UIGameResult Result { get; private set; }
        [field:SerializeField] public UIPause Pause { get; private set; }

        [SerializeField] private Button _pause;
        [SerializeField] private RectTransform _damageRoot;
        [SerializeField] private DamagePrefabs _damagePrefabs;
        [SerializeField] private string _damagePrefab;
        
        private PoolContainer _poolContainer;
        private InGameRepository _repository;
        private UnitDB _db;
        
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
            var uiDamage = await GetUIDamage(result);
            if (uiDamage == null)
                return;

            var tr = uiDamage.transform;
            tr.SetParent(_damageRoot);
            tr.localScale = Vector3.one;
            uiDamage.Container = _poolContainer;
            uiDamage.Show(result);
        }

        private async UniTask<UIDamage> GetUIDamage(AtkResult result)
        {
            // TODO : 데미지 타입에 따라서 프리팹 로드
            var pool = _poolContainer.Get(_damagePrefab);
            return await pool.Get() as UIDamage;
        }

        public void Init(PoolContainer poolContainer, InGameRepository repository, UnitDB db)
        {
            _poolContainer = poolContainer;
            _repository = repository;
            _db = db;

            _repository.recovers
                .ObserveAdd()
                .Subscribe(_=> OnRecoveryCollectionChanged())
                .AddTo(this);

            _repository.recovers
                .ObserveRemove()
                .Subscribe(_=> OnRecoveryCollectionChanged())
                .AddTo(this);
        }

        private async void OnRecoveryCollectionChanged()
        {
            var recovers = _repository.recovers;
            var info = recovers
                .Select(x => x.behaviour.Info)
                .ToArray();

            await DeadCharacters.Init(info, _db);
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

        public override void Dispose()
        {
            base.Dispose();
            
            DeadCharacters.Dispose();
        }
    }
}