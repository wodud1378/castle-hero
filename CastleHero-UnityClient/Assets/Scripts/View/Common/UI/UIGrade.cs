using CastleHero.Data.Model;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Common.UI
{
    public class UIGrade : MonoBehaviour
    {
        [FormerlySerializedAs("_legend")]
        [SerializeField] private GameObject legend;
        [FormerlySerializedAs("_epic")]
        [SerializeField] private GameObject epic;
        [FormerlySerializedAs("_rare")]
        [SerializeField] private GameObject rare;
        [FormerlySerializedAs("_common")]
        [SerializeField] private GameObject common;

        public void Set(EquipmentGrade grade)
        {
            legend.SetActive(grade == EquipmentGrade.Legend);
            epic.SetActive(grade == EquipmentGrade.Epic);
            rare.SetActive(grade == EquipmentGrade.Rare);
            common.SetActive(grade == EquipmentGrade.Common);
        }
    }
}
