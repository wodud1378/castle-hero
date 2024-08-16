using System;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Lobby.UI;
using RGLabs.Network.Shared;
using TMPro;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UIUserInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nickName;
        [SerializeField] private UISlot _portrait;
        [SerializeField] private UIItemSlot _dia;
        [SerializeField] private UIItemSlot _gold;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            var repository = Storage.userRepository;
            if(_nickName != null)
                _nickName.text = repository.nickname;

            if (_portrait != null)
            {
                if (!Storage.db.units.TryFind(repository.gameRecord.iconId.Value, out var entity))
                    entity = Storage.db.units[1];
            
                _portrait.Init(entity.icon).Forget();    
            }

            var currency = repository.currency;

            if (_gold != null)
            {
                currency.gold
                    .Subscribe(x =>
                    {
                        _gold.Init(new Item { ItemId = Constants.GoldId, Quantity = x }).Forget();
                    })
                    .AddTo(this);
            }

            if (_dia != null)
            {
                Observable.Merge(currency.freeDia, currency.paidDia)
                    .Subscribe(_ =>
                    {
                        int qty = currency.freeDia.Value + currency.paidDia.Value;
                        _dia.Init(new Item { ItemId = Constants.FreeDiaId, Quantity = qty }).Forget();
                    })
                    .AddTo(this);
            }
        }
    }
}