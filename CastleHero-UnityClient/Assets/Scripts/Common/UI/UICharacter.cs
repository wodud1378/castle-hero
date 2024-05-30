using RGLabs.Data;
using RGLabs.Network.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UICharacter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private GameObject[] _stars;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _lv;
        [SerializeField] private UIExp _exp;
        [SerializeField] private UIItemSlot[] _euipments;

        public void Init(UnitInfo info)
        {
            if (!Storage.db.units.TryFind(info.id, out var unitEntity))
                return;

            if (!Storage.db.balances.TryFind(info.id, out var balanceEntity))
                return;

            if (!Storage.db.levels.TryFind(info.lv, out var levelEntity))
                return;

            if (!Storage.db.rates.TryFind(info.rate, out var rateEntity))
                return;

            _lv.text = $"Lv.{info.lv}";
            _exp.Set(info.exp, levelEntity.exp);

            for (int i = 0, length = _stars.Length; i < length; ++i)
            {
                _stars[i].SetActive(info.rate < i);
            }
        }
    }
}