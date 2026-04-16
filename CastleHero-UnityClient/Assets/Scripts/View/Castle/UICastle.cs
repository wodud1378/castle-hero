using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;

using CastleHero.Data.DB;
namespace CastleHero.View.Castle
{
    public class UICastle : UIMain
    {
        [FormerlySerializedAs("_levelUpObjs")]
        [SerializeField] private List<GameObject> levelUpObjs;
        [FormerlySerializedAs("_requireGold")]
        [SerializeField] private TMP_Text requireGold;
        [FormerlySerializedAs("_skillList")]
        [SerializeField] private UIGlobalSkillDisplay skillList;
        [FormerlySerializedAs("_levelUp")]
        [SerializeField] private Button levelUp;

        private IUserRepository _repository;

        protected override void OnAwake()
        {
            base.OnAwake();

            _repository = ServiceLocator.Get<IUserRepository>();
            _repository.Currency.Gold
                .Subscribe(OnUpdateGold)
                .AddTo(this);

            _repository.GameRecord.CastleLv
                .Subscribe(UpdateUI)
                .AddTo(this);

            this.SubscribeButton(levelUp, OnClickLevelUp);
        }

        protected override void OnBack()
        {
            ServiceLocator.Get<StateManager<LobbyState>>().CurrentState = LobbyState.Main;
        }

        private void OnUpdateGold(int gold)
        {
            var lv = _repository.GameRecord.CastleLv.Value;
            if (!ServiceLocator.Get<IDBProvider>().Castles.TryFind(lv, out var entity))
                return;

            bool isEnough = gold >= entity.lvUpPrice;
            string text = $"{gold}/{entity.lvUpPrice}";
            requireGold.text = isEnough
                ? text.WithColor(Color.white)
                : text.WithNegativeColor();

            levelUp.interactable = isEnough;
            levelUp
                .GetComponent<UIGrayScale>()
                .enabled.Value = !isEnough;
        }

        private void UpdateUI(int castleLv)
        {
            var db = ServiceLocator.Get<IDBProvider>().Castles;
            if (!db.TryFind(castleLv, out var entity))
                return;

            int maxLv = db.TryIndexOf(db.Length - 1, out var maxLvEntity)
                ? maxLvEntity.Id
                : 100;

            bool isNotMaxLv = castleLv < maxLv;
            levelUpObjs.ForEach(x => x.gameObject.SetActive(isNotMaxLv));
            levelUp.gameObject.SetActive(isNotMaxLv);
            skillList
                .Init(entity.SkillParameters())
                .Forget();
        }

        private async UniTask OnClickLevelUp()
        {
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Castle.LvUp();
            if (!result.IsSuccess)
            {
                ServiceLocator.Get<IPopupManager>().Open<PopupCommon>(result.error);
                return;
            }

            ServiceLocator.Get<ISoundManager>().PlaySfx(ServiceLocator.Get<SoundPath>().levelUp);
        }
    }
}
