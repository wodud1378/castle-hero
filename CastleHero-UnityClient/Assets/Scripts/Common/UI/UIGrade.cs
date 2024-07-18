using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UIGrade : MonoBehaviour
    {
        [SerializeField] private GameObject _legend;
        [SerializeField] private GameObject _epic;
        [SerializeField] private GameObject _rare;
        [SerializeField] private GameObject _common;

        public void Set(EquipmentGrade grade)
        {
            _legend.SetActive(grade == EquipmentGrade.Legend);
            _epic.SetActive(grade == EquipmentGrade.Epic);
            _rare.SetActive(grade == EquipmentGrade.Rare);
            _common.SetActive(grade == EquipmentGrade.Common);
        }
    }
}