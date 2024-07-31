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

        public void Init()
        {
            var repository = Storage.userRepository;
            _nickName.text = repository.nickname;

            if (!Storage.db.units.TryFind(repository.profileCharacter, out var entity))
                entity = Storage.db.units[0];
            
            _portrait.Init(entity.icon).Forget();

            repository.gold
                .Subscribe(x =>
                {
                    _gold.Init(new Item { ItemId = Constants.GoldId, Quantity = x }).Forget();
                })
                .AddTo(this);

            Observable.Merge(repository.freeDia, repository.paidDia)
                .Subscribe(_ =>
                {
                    int qty = repository.freeDia.Value + repository.paidDia.Value;
                    _dia.Init(new Item { ItemId = Constants.FreeDiaId, Quantity = qty }).Forget();
                })
                .AddTo(this);
        }
    }
}