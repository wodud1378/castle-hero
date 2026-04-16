using CastleHero.View.Common.UI;
using CastleHero.View.Lobby.Shop.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CastleHero.Editor
{
    public static class ProductHelper
    {
        [MenuItem("Assets/CastleHero/Replace Slot To Price")]
        public static void ReplaceSlotToPrice()
        {
            var selection = Selection.assetGUIDs;
            if (selection == null || selection.Length == 0)
                return;

            foreach (var guid in selection)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null)
                {
                    Debug.LogWarning($"Prefab at path {prefabPath} not found.");
                    continue;
                }

                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                if (prefabInstance == null)
                {
                    Debug.LogWarning($"Failed to instantiate prefab at path {prefabPath}.");
                    continue;
                }

                if (prefabInstance.TryGetComponent(out UIShopItemSlot slot))
                {
                    //slot._grayScaleOnDim = true;
                    var root = prefabInstance.transform;
               
                    var priceTr = root.Find("Price");
                    if (priceTr.TryGetComponent(out UISlot legacy))
                    {
                        Object.DestroyImmediate(legacy);
                    }
                    
                    if (!priceTr.TryGetComponent(out UIPrice price))
                    {
                        price = priceTr.gameObject.AddComponent<UIPrice>();
                    }

                    slot.price = price;
                }
                
                PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);
                Object.DestroyImmediate(prefabInstance);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/CastleHero/Set Schedule Texts")]
        public static void SetScheduleTexts()
        {
            var selection = Selection.assetGUIDs;
            if (selection == null || selection.Length == 0)
                return;

            foreach (var guid in selection)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null)
                {
                    Debug.LogWarning($"Prefab at path {prefabPath} not found.");
                    continue;
                }

                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                if (prefabInstance == null)
                {
                    Debug.LogWarning($"Failed to instantiate prefab at path {prefabPath}.");
                    continue;
                }

                if (prefabInstance.TryGetComponent(out UIShopItemSlot slot))
                {
                    //slot._grayScaleOnDim = true;
                    var root = prefabInstance.transform;
                    var timerRoot = root.Find("Timer");
                    var content = root.Find("Mask");
                    timerRoot ??= content.Find("Timer");
                    if (timerRoot != null)
                    {

                        var leftTime = timerRoot.GetComponent<TMP_Text>();
                        leftTime ??= timerRoot.GetComponent<TMP_Text>();
                        leftTime ??= timerRoot.Find("Timer").GetComponent<TMP_Text>();

                        slot.leftTimeForReset = leftTime; 
                    }

                    var expireRoot = content.Find("State");
                    if (expireRoot != null)
                    {
                        var expire = expireRoot.Find("Label").GetComponent<TMP_Text>();

                        slot.leftTimeForExpire = expire;
                    }
                }
                
                PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);
                Object.DestroyImmediate(prefabInstance);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/CastleHero/Attach Product Component")]
        public static void AttachProductComponent()
        {
            var selection = Selection.assetGUIDs;
            if (selection == null || selection.Length == 0)
                return;

            foreach (var guid in selection)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null)
                {
                    Debug.LogWarning($"Prefab at path {prefabPath} not found.");
                    continue;
                }

                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                if (prefabInstance == null)
                {
                    Debug.LogWarning($"Failed to instantiate prefab at path {prefabPath}.");
                    continue;
                }

                if (!prefabInstance.TryGetComponent(out UIShopItemSlot slot))
                {
                    slot = prefabInstance.AddComponent<UIShopItemSlot>();
                    //slot._grayScaleOnDim = true;
                    var root = prefabInstance.transform;
                    var content = root.Find("Mask");
                    var grayScale = prefabInstance.AddComponent<UIGrayScale>();
                    
                    grayScale.targets ??= new Graphic[1];
                    grayScale.targets[0] = content.Find("Image").GetComponent<Image>();
                    slot.grayScale = grayScale;

                    var nameRoot = content.Find("Name");
                    if(nameRoot != null)
                        slot.label = nameRoot.GetComponent<TMP_Text>();

                    var priceTr = root.Find("Price");
                    if (!priceTr.TryGetComponent(out UIPrice price))
                    {
                        price = priceTr.gameObject.AddComponent<UIPrice>();
                    }

                    slot.price = price;

                    var timerRoot = content.Find("Timer");
                    if (timerRoot != null)
                    {
                        var leftTime = content.Find("Timer").GetComponent<TMP_Text>();
                        leftTime ??= content.Find("Timer").Find("Timer").GetComponent<TMP_Text>();

                        slot.leftTimeForExpire = leftTime; 
                    }
                    
                    slot.leftCount = content.Find("Count").Find("Label").GetComponent<TMP_Text>();
                }
                
                PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);
                Object.DestroyImmediate(prefabInstance);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}