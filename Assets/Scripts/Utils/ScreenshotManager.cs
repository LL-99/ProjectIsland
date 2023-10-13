using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Helper class to take screenshots ingame or in play mode
/// </summary>
public class ScreenshotManager : MonoBehaviour
{
    #region Singleton
    public static ScreenshotManager Instance { get; private set; }

    public void EnforceSingleton()
    {
        if (!Instance) Instance = this;
        else if (Instance != this) Destroy(this);
    }
    #endregion

    private void Awake()
    {
        EnforceSingleton();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F12))
            TakeScreenshot();
    }

    public void TakeScreenshot(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        TakeScreenshot();
    }

    /// <summary>
    /// Take a screenshot and save it
    /// </summary>
    public void TakeScreenshot() 
    { 
        var now = System.DateTime.Now;

        // How the hell does this syntax work? Yeah this is horrible code, but I think it looks hilarious, so I'm gonna keep it
        var filename = $"{$"{now.ToShortDateString()}_{now.ToLongTimeString()}".Replace('.', '_').Replace(':', '_')}.png";


#if UNITY_EDITOR
        var path = Application.dataPath + "/../Screenshots";
#else
        var path = Application.persistentDataPath + "/Screenshots";    
#endif

        if(!System.IO.Directory.Exists(path)) {
            Debug.Log("Creating new screenshot directory: " + path);
            System.IO.Directory.CreateDirectory(path);
        }

        Debug.Log($"Capturing screenshot as '{filename}' in '{path}'");
        path += "/" + filename;

        ScreenCapture.CaptureScreenshot(path);
    }
}
