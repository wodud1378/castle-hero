using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public struct GameEntrance
    {
        public GameType type;
        public int id;
    }

    public class UserRepository : IDisposable
    {
        public readonly string nickname;

        public readonly ReactiveProperty<GameEntrance> entrance;

        public int StageFocus
        {
            get => PlayerPrefs.GetInt(StageFocusKey, Mathf.Max(1, gameRecord.lastClearedStage.Value));
            private set => PlayerPrefs.SetInt(StageFocusKey, value);
        }

        public readonly Stamina stamina;
        public readonly Currency currency;
        public readonly Inventory inventory;
        public readonly Characters characters;
        public readonly Formation formation;
        public readonly GameRecord gameRecord;
        public readonly ShopRecord shopRecord;

        private const string StageFocusKey = "stage-focus";

        public UserRepository(string nickname, UserDataDto dto)
        {
            this.nickname = nickname;

            stamina = new(dto.stamina);
            currency = new(dto.currency);
            inventory = new(dto.inventory);
            characters = new(dto.characters);
            formation = new(dto.formation);
            gameRecord = new(dto.gameRecord);
            shopRecord = new(dto.shopRecord);

            entrance = new(new GameEntrance
            {
                type = GameType.Stage,
                id = StageFocus
            });

            entrance
                .Subscribe(x =>
                {
                    if (x.type != GameType.Stage)
                        return;

                    StageFocus = x.id;
                });
        }

        public void Update(UserDataDto dto)
        {
            if (dto.stamina != null)
                stamina.Update(dto.stamina);

            if (dto.currency != null)
                currency.Update(dto.currency);
            
            if (dto.inventory != null)
                inventory.Update(dto.inventory);
            
            if (dto.characters != null)
                characters.Update(dto.characters);
            
            if (dto.formation != null)
                formation.Update(dto.formation);
            
            if (dto.gameRecord != null)
                gameRecord.Update(dto.gameRecord);
            
            if (dto.shopRecord != null)
                shopRecord.Update(dto.shopRecord);
        }

        public IEnumerable<EquipItem> EquipItems(IList<string> guids)
        {
            return inventory.items
                .OfType<EquipItem>()
                .Where(x => guids.Contains(x.Guid));
        }

        public void Dispose()
        {
            entrance?.Dispose();
            stamina?.Dispose();
            currency?.Dispose();
            inventory?.Dispose();
            characters?.Dispose();
            formation?.Dispose();
            gameRecord?.Dispose();
            shopRecord?.Dispose();
        }
    }
}