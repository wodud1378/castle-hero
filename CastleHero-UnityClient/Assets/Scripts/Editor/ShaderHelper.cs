using System;
using UnityEditor;
using UnityEngine;

namespace CastleHero.Editor
{
    public static class ShaderHelper
    {
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Blend = Shader.PropertyToID("_Blend");

        private enum BlendMode
        {
            Additive,
            AlphaBlended,
        }

        [MenuItem("Tools/CastleHero/Migrate Particle Shader To Urp Shader")]
        private static void MigrateShaders()
        {
            // 모든 머티리얼을 찾습니다.
            string[] guids = AssetDatabase.FindAssets("t:Material");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (material != null)
                {
                    // 기존 쉐이더 이름을 기반으로 교체 작업을 수행합니다.
                    Shader shader = material.shader;
                    if (!shader.name.StartsWith("Mobile/Particles"))
                        continue;
                    
                    var blendMode = GetBlendMode(shader);
                    
                    MigrateToURPShader(material, blendMode);
                    
                    Debug.Log($"{material.name} migrated to urp shader.");
                }
            }

            // 모든 변경 사항을 저장합니다.
            AssetDatabase.SaveAssets();
            Debug.Log("Shader migration completed.");
        }

        private static BlendMode GetBlendMode(Shader shader)
        {
            var name = shader.name;
            return name.Contains("Additive") 
                ? BlendMode.Additive 
                : name.Contains("Alpha Blended") 
                    ? BlendMode.AlphaBlended : default;
        }

        private static void MigrateToURPShader(Material material, BlendMode blendMode)
        {
            // 새로운 URP 쉐이더로 변경합니다.
            var urpShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (urpShader != null)
            {
                material.shader = urpShader;

                // 메인 텍스처를 다시 할당합니다.
                material.SetTexture(BaseMap, material.GetTexture(MainTex));

                // Transparent 설정
                material.SetFloat(Surface, 1.0f); // Transparent

                // 블렌드 모드 설정
                switch (blendMode)
                {
                    case BlendMode.Additive:
                        material.SetInt(Surface, 1);
                        material.SetInt(Blend, 2);
                        break;
                    case BlendMode.AlphaBlended:
                        material.SetInt(Surface, 1);
                        material.SetInt(Blend, 0);
                        break;
                }

                // 변경사항 저장
                EditorUtility.SetDirty(material);
            }
        }
    }
}