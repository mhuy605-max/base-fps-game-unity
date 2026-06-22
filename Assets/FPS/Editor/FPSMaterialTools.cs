using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    public static class FPSMaterialTools
    {
        [MenuItem("FPS/Fix Snow Material")]
        public static void FixSnowMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/FPS/Materials/Snow.mat");
            Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FPS/Materials/snow_01_diff_4k.jpg");

            if (material == null)
            {
                Debug.LogError("Could not find Assets/FPS/Materials/Snow.mat");
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("Could not find shader: Universal Render Pipeline/Lit");
                return;
            }

            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.2f);

            if (baseMap != null)
            {
                material.SetTexture("_BaseMap", baseMap);
                material.SetTextureScale("_BaseMap", new Vector2(8f, 8f));
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset("Assets/FPS/Materials/Snow.mat", ImportAssetOptions.ForceUpdate);
            Debug.Log("Snow material rebuilt with Universal Render Pipeline/Lit.");
        }
    }
}
