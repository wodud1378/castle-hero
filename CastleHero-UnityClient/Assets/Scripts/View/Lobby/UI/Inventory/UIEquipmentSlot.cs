using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI.Inventory
{
    public class UIEquipmentSlot : UIItemSlot
    {
        private static readonly Dictionary<ElementalType, string> ElementIcons = new()
        {
            { ElementalType.Earth, "Sprites/Global/UI/Symbol_Elemental_Earth_Slot.png" },
            { ElementalType.Fire, "Sprites/Global/UI/Symbol_Elemental_Fire_Slot.png" },
            { ElementalType.Wind, "Sprites/Global/UI/Symbol_Elemental_Wind_Slot.png" },
            { ElementalType.Water, "Sprites/Global/UI/Symbol_Elemental_Water_Slot.png" },
        };

        [FormerlySerializedAs("_grade")]
        [SerializeField] private UIGrade grade;
        [FormerlySerializedAs("_portraitRoot")]
        [SerializeField] private GameObject portraitRoot;
        [FormerlySerializedAs("_portrait")]
        [SerializeField] private AddressableImage portrait;
        [FormerlySerializedAs("_element")]
        [SerializeField] private AddressableImage element;

        public void Init(EquipItem item)
        {
            if (!_db.Items.TryFind(item.ItemId, out var entity))
                return;

            Init(item, entity);
        }

        public void Init(EquipItem item, ItemEntity entity)
        {
            var option = entity.optionEquip;
            if (grade != null)
                grade.Set(option.grade);

            if (!ElementIcons.TryGetValue((ElementalType)item.element.type, out var elementPath))
                elementPath = string.Empty;

            string portraitPath = _db.Units.TryFind(item.character, out var e) ? e.icon : string.Empty;

            base.Init(item, entity);
            UpdatePortrait(portraitPath).SafeForget();
            element.Set(elementPath).SafeForget();
        }

        private UniTask UpdatePortrait(string portraitPath)
        {
            bool hasPortrait = !string.IsNullOrEmpty(portraitPath);
            portraitRoot.SetActive(hasPortrait);

            if (!hasPortrait)
                return UniTask.CompletedTask;

            return portrait.Set(portraitPath);
        }

        public override void Dispose()
        {
            base.Dispose();

            portrait.Dispose();
            element.Dispose();
        }
    }
}
