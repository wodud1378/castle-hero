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

        public void Set(IngredientGradeCode grade) { }

        public void Set(EquipmentGradeCode grade)
        {
            _legend.SetActive(grade == EquipmentGradeCode.Legend);
            _epic.SetActive(grade == EquipmentGradeCode.Epic);
            _rare.SetActive(grade == EquipmentGradeCode.Rare);
            _common.SetActive(grade == EquipmentGradeCode.Common);
        }
    }
}