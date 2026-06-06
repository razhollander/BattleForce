#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Forces the Game View into a borderless, true fullscreen popup when entering Play Mode.
/// </summary>
[InitializeOnLoad]
public static class FullscreenPlayMode
{
    // Use Reflection to get the internal GameView type
    private static readonly Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    
    // Find the internal property to hide the toolbar
    private static readonly PropertyInfo showToolbarProperty = gameViewType?.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);
    
    private static EditorWindow fullscreenInstance;

    // Register to the play mode state change event on load
    static FullscreenPlayMode()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EnterFullscreen();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            ExitFullscreen();
        }
    }

    // You can also toggle this manually using Ctrl+F11 (or Cmd+F11 on Mac)
    [MenuItem("Window/General/Toggle Fullscreen Play Mode %F11", priority = 2)]
    public static void Toggle()
    {
        if (fullscreenInstance != null)
            ExitFullscreen();
        else
            EnterFullscreen();
    }

    private static void EnterFullscreen()
    {
        if (gameViewType == null)
        {
            Debug.LogError("FullscreenPlayMode: UnityEditor.GameView type not found.");
            return;
        }

        // Clean up any lingering instance
        if (fullscreenInstance != null)
        {
            fullscreenInstance.Close();
        }

        // Create a new GameView window
        fullscreenInstance = (EditorWindow)ScriptableObject.CreateInstance(gameViewType);
        
        // Hide the toolbar if we successfully found the property
        showToolbarProperty?.SetValue(fullscreenInstance, false);

        // Get the current monitor's resolution
        var desktopResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        var fullscreenRect = new Rect(Vector2.zero, desktopResolution);

        // Show as a borderless popup and stretch to fill the screen
        fullscreenInstance.ShowPopup();
        fullscreenInstance.position = fullscreenRect;
        fullscreenInstance.Focus();
    }

    private static void ExitFullscreen()
    {
        if (fullscreenInstance != null)
        {
            fullscreenInstance.Close();
            fullscreenInstance = null;
        }
    }
}
#endif