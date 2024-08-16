using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Network.Service;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Castle
{
    public class UICastle : UIMain
    {
        [SerializeField] private List<GameObject> _levelUpObjs;
        [SerializeField] private TMP_Text _requireGold;
        [SerializeField] private UIGlobalSkillDisplay _skillList;
        [SerializeField] private Button _levelUp;
        [SerializeField] private Button _back;

        private UserRepository _repository;
        
        private void Awake()
        {
            _repository = Storage.userRepository;
            _repository.currency.gold
                .Subscribe(OnUpdateGold)
                .AddTo(this);
            
            _repository.gameRecord.castleLv
                .Subscribe(UpdateUI)
                .AddTo(this);
            
            this.SubscribeButton(_back, OnBack);
            this.SubscribeButton(_levelUp, OnClickLevelUp);
        }

        private void OnBack()
        {
            Context.Transition.CurrentState = State.Lobby;
        }

        private void OnUpdateGold(int gold)
        {
            var lv = _repository.gameRecord.castleLv.Value;
            if (!Storage.db.castles.TryFind(lv, out var entity))
                return;

            bool isEnough = gold >= entity.lvUpPrice;
            string text = $"{gold}/{entity.lvUpPrice}";
            _requireGold.text = isEnough
                ? text.WithColor(Color.white)
                : text.WithNegativeColor();
            
            _levelUp.interactable = isEnough;
            _levelUp
                .GetComponent<UIGrayScale>()
                .enabled.Value = !isEnough;
        }
        
        private void UpdateUI(int castleLv)
        {
            var db = Storage.db.castles;
            if (!db.TryFind(castleLv, out var entity))
                return;

            int maxLv = db.TryIndexOf(db.Length - 1, out var maxLvEntity)
                ? maxLvEntity.Id
                : 100;

            bool isNotMaxLv = castleLv < maxLv;
            _levelUpObjs.ForEach(x => x.gameObject.SetActive(isNotMaxLv));     
            _levelUp.gameObject.SetActive(isNotMaxLv);
            _skillList
                .Init(entity.SkillParameters())
                .Forget();
        }

        private async void OnClickLevelUp()
        {
            var result = await NetworkService.Castle.LevelUp();
            
            _repository.gameRecord.castleLv.Value = result.lv;
            _repository.currency.Update(result.leftCurrency);
        }
    }
}