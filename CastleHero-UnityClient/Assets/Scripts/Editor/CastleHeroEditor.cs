using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CastleHero.Editor
{
    public static class CastleHeroEditor
    {
        public static void ModifyAllPrefabsWithComponent<T>(Action<T> modifyAction) where T : Component
        {
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            foreach (string prefabPath in prefabPaths)
            {
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

                T component = prefabInstance.GetComponentInChildren<T>();
                if (component == null)
                {
                    Object.DestroyImmediate(prefabInstance);
                    continue;
                }

                modifyAction(component);

                PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);

                Object.DestroyImmediate(prefabInstance);

                Debug.Log($"Modified prefab at path {prefabPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}