using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RGLabs.Editor
{
    public static class ComponentHelper
    {
        [MenuItem("Tools/RGLabs/Remove Missing Components")]
        public static void RemoveMissingComponents()
        {
            // 1. 씬을 순회하면서 Missing 컴포넌트 제거
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    string scenePath = scene.path;
                    EditorSceneManager.OpenScene(scenePath);

                    // 모든 게임 오브젝트를 가져와서 Missing 컴포넌트 제거
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        RemoveMissingComponentsFromGameObject(obj);
                    }

                    // 변경 사항 저장
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }
            }

            // 2. 프리팹을 순회하면서 Missing 컴포넌트 제거
            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            foreach (string prefabPath in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    RemoveMissingComponentsFromGameObject(prefab);

                    // 프리팹 저장
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Missing component removal complete.");
        }

        private static void RemoveMissingComponentsFromGameObject(GameObject obj)
        {
            // SerializedObject를 사용하여 컴포넌트 리스트를 가져옴
            SerializedObject serializedObject = new SerializedObject(obj);
            SerializedProperty prop = serializedObject.FindProperty("m_Component");

            if (prop != null && prop.isArray)
            {
                for (int i = prop.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty componentProperty = prop.GetArrayElementAtIndex(i);
                    try
                    {
                        if (componentProperty.objectReferenceValue == null)
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            Debug.Log($"Removed missing component({componentProperty.displayName}) from: {obj.name}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Removed failed({componentProperty.displayName}) from: {obj.name}, {e.ToString()}");
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        public static void RemoveSpecificComponent<T>() where T : Component
        {
            // 1. 씬을 순회하면서 컴포넌트 제거
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    string scenePath = scene.path;
                    EditorSceneManager.OpenScene(scenePath);

                    // 모든 게임 오브젝트를 가져와서 컴포넌트 제거
                    GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        var component = obj.GetComponent<T>(); // YourComponent를 제거하려는 컴포넌트로 변경
                        if (component != null)
                        {
                            Object.DestroyImmediate(component);
                            Debug.Log($"Removed component from: {obj.name} in scene {scenePath}");
                        }
                    }

                    // 변경 사항 저장
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
            }

            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            foreach (string prefabPath in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    var components = prefab.GetComponentsInChildren<T>(true); // 모든 자식 오브젝트 포함하여 검색
                    foreach (var component in components)
                    {
                        Object.DestroyImmediate(component, true);
                        Debug.Log($"Removed component from prefab: {prefab.name}");

                        // 프리팹 저장
                        PrefabUtility.SavePrefabAsset(prefab);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Component removal complete.");
        }
    }
}