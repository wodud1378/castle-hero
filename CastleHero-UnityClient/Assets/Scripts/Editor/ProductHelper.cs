using RGLabs.Common.UI;
using RGLabs.Lobby.Shop.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Editor
{
    public static class ProductHelper
    {
        [MenuItem("Assets/RGLabs/Attach Product Component")]
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
                    if (!priceTr.TryGetComponent(out UISlot price))
                    {
                        price = priceTr.gameObject.AddComponent<UISlot>();
                        price.icon = priceTr.Find("Icon").GetComponent<Image>();
                        price.label = priceTr.Find("Value").GetComponent<TMP_Text>();
                    }

                    slot.price = price;

                    var timerRoot = content.Find("Timer");
                    if (timerRoot != null)
                    {
                        var leftTime = content.Find("Timer").GetComponent<TMP_Text>();
                        leftTime ??= content.Find("Timer").Find("Timer").GetComponent<TMP_Text>();

                        slot.leftTime = leftTime; 
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