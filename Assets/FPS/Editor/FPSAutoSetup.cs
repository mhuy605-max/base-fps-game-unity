using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Builds the FPS scene automatically the first time Unity Editor opens this project.
    /// Unity Hub alone cannot run this — you must open the project in the Editor.
    /// </summary>
    [InitializeOnLoad]
    public static class FPSAutoSetup
    {
        const string ScenePath = "Assets/Scenes/FPS_Scene.unity";
        const string PrefKey = "FPSGame.SceneCreated.v1";

        static FPSAutoSetup()
        {
            EditorApplication.delayCall += TryCreateSceneOnce;
        }

        static void TryCreateSceneOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (File.Exists(ScenePath))
                return;

            if (SessionState.GetBool(PrefKey, false))
                return;

            SessionState.SetBool(PrefKey, true);

            FPSGameSetup.CreateFPSScene(openAfterCreate: true);

            EditorUtility.DisplayDialog(
                "FPS game ready",
                "The FPS arena scene was created and opened.\n\n" +
                "Press the Play button (top center) to play.\n\n" +
                "Controls: WASD, mouse, left-click shoot, R reload.",
                "OK");
        }
    }
}
