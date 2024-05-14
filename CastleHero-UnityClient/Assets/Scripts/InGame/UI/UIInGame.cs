using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Pattern;
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

        public void Init(PoolContainer poolContainer)
        {
            _poolContainer = poolContainer;

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

        public override void Dispose()
        {
            base.Dispose();
            
            DeadCharacters.Dispose();
        }
    }
}