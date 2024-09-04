using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIEquipmentSlot : UIItemSlot
    {
        private static readonly Dictionary<Elemental.Type, string> ElementIcons = new()
        {
            { Elemental.Type.Earth, "Sprites/Global/UI/Symbol_Elemental_Earth_Slot.png" },
            { Elemental.Type.Fire, "Sprites/Global/UI/Symbol_Elemental_Fire_Slot.png" },
            { Elemental.Type.Wind, "Sprites/Global/UI/Symbol_Elemental_Wind_Slot.png" },
            { Elemental.Type.Water, "Sprites/Global/UI/Symbol_Elemental_Water_Slot.png" },
        };

        [SerializeField] private UIGrade _grade;
        [SerializeField] private GameObject _portraitRoot;
        [SerializeField] private AddressableImage _portrait;
        [SerializeField] private AddressableImage _element;

        public UniTask Init(EquipItem item)
        {
            if (!Storage.db.items.TryFind(item.ItemId, out var entity))
                return UniTask.CompletedTask;

            return Init(item, entity);
        }

        public UniTask Init(EquipItem item, ItemEntity entity)
        {
            var option = entity.optionEquip;
            if (_grade != null)
                _grade.Set(option.grade);

            if (!ElementIcons.TryGetValue((Elemental.Type)item.element.type, out var element))
                element = string.Empty;

            string portrait = Storage.db.units.TryFind(item.character, out var e) ? e.icon : string.Empty;
            return UniTask.WhenAll(
                base.Init(item, entity),
                UpdatePortrait(portrait),
                _element.Set(element));
        }

        private UniTask UpdatePortrait(string portrait)
        {
            bool hasPortrait = !string.IsNullOrEmpty(portrait);
            _portraitRoot.SetActive(hasPortrait);

            if (!hasPortrait)
                return UniTask.CompletedTask;

            return _portrait.Set(portrait);
        }

        public override void Dispose()
        {
            base.Dispose();

            _portrait.Dispose();
            _element.Dispose();
        }
    }
}