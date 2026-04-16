using System.Collections.Generic;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using Cysharp.Threading.Tasks;
namespace CastleHero.View.Test
{
    public class UITest : MonoBehaviour
    {
        [FormerlySerializedAs("_root")]
        [SerializeField] private GameObject root;
        [FormerlySerializedAs("_closeButton")]
        [SerializeField] private Button closeButton;

        [Header("아이템 추가")]
        [FormerlySerializedAs("_itemId")]
        [SerializeField] private TMP_InputField itemId;
        [FormerlySerializedAs("_itemQty")]
        [SerializeField] private TMP_InputField itemQty;
        [FormerlySerializedAs("_addItemButton")]
        [SerializeField] private Button addItemButton;
        [FormerlySerializedAs("_addItemLog")]
        [SerializeField] private TMP_Text addItemLog;

        [Header("재화 추가")]
        [FormerlySerializedAs("_paidDia")]
        [SerializeField] private TMP_InputField paidDia;
        [FormerlySerializedAs("_freeDia")]
        [SerializeField] private TMP_InputField freeDia;
        [FormerlySerializedAs("_gold")]
        [SerializeField] private TMP_InputField gold;
        [FormerlySerializedAs("_addCurrencyButton")]
        [SerializeField] private Button addCurrencyButton;
        [FormerlySerializedAs("_addCurrencyLog")]
        [SerializeField] private TMP_Text addCurrencyLog;

        private ITestService _service;

        private bool IsOpened => root.activeSelf;

        private void Awake()
        {
            _service = ServiceLocator.TryGet<ITestService>(out var svc) ? svc : null;

            this.SubscribeButton(closeButton, Close);
            this.SubscribeButton(addItemButton, OnClickAddItems);
            this.SubscribeButton(addCurrencyButton, OnClickAddCurrency);

            Close();
        }

        private async UniTask OnClickAddItems()
        {
            if (_service == null)
            {
                addItemLog.text = "ITestService 미등록".WithNegativeColor();
                return;
            }

            var ids = itemId.text.Trim().Split(',');
            var quantities = itemQty.text.Trim().Split(',');
            var idList = new List<int>();
            var quantityList = new List<int>();

            int index = 0;
            while (index.IsValidIndex(ids, quantities))
            {
                if (int.TryParse(ids[index], out int id) &&
                    int.TryParse(quantities[index], out int quantity))
                {
                    idList.Add(id);
                    quantityList.Add(quantity);
                }

                ++index;
            }

            if (idList.Count == 0 || quantityList.Count == 0)
            {
                addItemLog.text = "입력 오류".WithNegativeColor();
                return;
            }

            addItemLog.text = "요청 진행 중".WithColor(Color.gray);

            var result = await _service.AddItems(idList.ToArray(), quantityList.ToArray());

            addItemLog.text = result.IsSuccess
                ? "요청 성공".WithPositiveColor()
                : $"에러 발생 [{result.error}]";
        }

        private async UniTask OnClickAddCurrency()
        {
            if (_service == null)
            {
                addCurrencyLog.text = "ITestService 미등록".WithNegativeColor();
                return;
            }

            int temp;
            int paidDiaVal = int.TryParse(paidDia.text, out temp) ? temp : 0;
            int freeDiaVal = int.TryParse(freeDia.text, out temp) ? temp : 0;
            int goldVal = int.TryParse(gold.text, out temp) ? temp : 0;

            if (paidDiaVal == 0 && freeDiaVal == 0 && goldVal == 0)
            {
                addCurrencyLog.text = "입력 오류".WithNegativeColor();
            }
            else
            {
                addCurrencyLog.text = "요청 진행 중".WithColor(Color.gray);
                var currency = new CurrencyDto { paidDia = paidDiaVal, freeDia = freeDiaVal, gold = goldVal, };

                var result = await _service.AddCurrency(currency);

                addCurrencyLog.text = result.IsSuccess
                    ? "요청 성공".WithPositiveColor()
                    : $"에러 발생 [{result.error}]".WithNegativeColor();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && IsOpened)
            {
                Close();
            }

            if (Input.GetKeyDown(KeyCode.F1) && !IsOpened)
            {
                Open();
            }
        }

        public void Open() => root.SetActive(true);

        private void Close() => root.SetActive(false);
    }
}
