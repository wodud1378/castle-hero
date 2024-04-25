using System;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    [Serializable]
    public class UIStage : IDisposable
    {
        public TMP_Text title;
        public UIIconTextSet[] items;

        private MonoBehaviour _owner;
        private RewardDB _rewardDB;
        private ItemDB _itemDB;
        
        public void Init(MonoBehaviour owner, RewardDB rewardDB, ItemDB itemDB)
        {
            _owner = owner;
            _rewardDB = rewardDB;
            _itemDB = itemDB;
        }
        
        public async void Set(StageEntity data)
        {
            var rewards = _rewardDB.Map(data.rewards);

            int uiCount = items.Length;
            int dataCount = rewards.Length;
            int stage = data.Id;

            title.text = $"STAGE {stage}";
            
            for (int i = 0; i < uiCount; ++i)
            {
                if (i < dataCount)
                {
                    var reward = rewards[i];
                    if (!_itemDB.TryFind(reward.itemId, out var entity))
                        continue;

                    var handle = await entity.icon.Handle<Sprite>();
                    string text = reward.minQuantity == reward.maxQuantity
                        ? $"{reward.maxQuantity}"
                        : $"{reward.minQuantity} ~ {reward.maxQuantity}";

                    items[i].Set(handle, text);
                    items[i].SetActive(true);
                }
                else
                {
                    items[i].SetActive(false);
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in items)
            {
                item.Dispose();
            }
        }
    }

    [Serializable]
    public class UIUser : UIIconTextSet
    {
    }

    [Serializable]
    public class UIWealth : UIIconTextSet
    {
    }

    [Serializable]
    public class UIIconTextSet : IDisposable
    {
        public Image icon;
        public TMP_Text label;

        private AsyncOperationHandle<Sprite> _spriteHandle;

        public void Set(AsyncOperationHandle<Sprite> handle, string text)
        {
            _spriteHandle = handle;

            Set(handle.Result, text);
        }

        public void Dispose() => _spriteHandle.Release();

        private void Set(Sprite sprite, string text)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            //label.text = text;
        }

        public void Fallback() => Set(null, string.Empty);

        public void SetActive(bool isActive)
        {
            icon.gameObject.SetActive(isActive);
            //label.gameObject.SetActive(isActive);
        }
    }
}