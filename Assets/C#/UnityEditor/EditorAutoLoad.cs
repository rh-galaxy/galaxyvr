#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

// http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor

[InitializeOnLoad]
public class AutoPlayModeSceneSetup
{
    static AutoPlayModeSceneSetup()
    {
        // Ensure at least one build scene exist.
        if (EditorBuildSettings.scenes.Length == 0)
            return;

        // Set Play Mode scene to first scene defined in build settings.
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
    }
}

[InitializeOnLoad]
public class CompilerOptionsEditorScript
{
    static bool waitingForStop = false;

    static CompilerOptionsEditorScript()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    static void OnEditorUpdate()
    {
        if (!waitingForStop
            && EditorApplication.isCompiling
            && EditorApplication.isPlaying)
        {
            EditorApplication.LockReloadAssemblies();
            EditorApplication.playmodeStateChanged
                 += PlaymodeChanged;
            waitingForStop = true;
        }
    }

    static void PlaymodeChanged()
    {
        if (EditorApplication.isPlaying)
            return;

        EditorApplication.UnlockReloadAssemblies();
        EditorApplication.playmodeStateChanged
             -= PlaymodeChanged;
        waitingForStop = false;
    }
}

#endif
