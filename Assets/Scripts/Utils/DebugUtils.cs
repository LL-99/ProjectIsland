using UnityEngine;

/// <summary>
/// Contains utility functions for easier debugging
/// </summary>
public static class DebugUtils
{
    /// <summary>
    /// Draws a string at a specific position for debug purposes
    /// Found at: https://gist.github.com/Arakade/9dd844c2f9c10e97e3d0?permalink_comment_id=4043513#gistcomment-4043513
    /// </summary>
    /// <param name="text">Text to show</param>
    /// <param name="worldPosition">Where to show text</param>
    /// <param name="textColor">Color of the text</param>
    /// <param name="anchor">Text alignment; (0,0) = center, (1,1) = bottom-right, etc</param>
    /// <param name="textSize">Font size</param>
    public static void DrawString(string text, Vector3 worldPosition, Color textColor, Vector2 anchor, float textSize = 15f)
    {
        #if UNITY_EDITOR

        // Get the scene view for purposes of screen space conversion
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if (!view)
            return;

        // Get the screen position to draw the handle at
        Vector3 screenPosition = view.camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.y < 0 || 
            screenPosition.y > view.camera.pixelHeight || 
            screenPosition.x < 0 || 
            screenPosition.x > view.camera.pixelWidth || 
            screenPosition.z < 0)
            return;

        // Calculate UI scaling by comparing the screen pixel conversion delta (may be equivalent to EditorGUIUtility.pixelsPerPoint)
        var pixelRatio = UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x - UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;
        
        // Begin drawing the GUI handle
        UnityEditor.Handles.BeginGUI();
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = (int)textSize,
            normal = new GUIStyleState() { textColor = textColor },
        };

        // Calculate the label size based on pixel ratio and font size
        Vector2 size = style.CalcSize(new GUIContent(text)) * pixelRatio;

        // Calculate the actual position in screen space
        var alignedPosition =
            ((Vector2)screenPosition +
            size * ((anchor + Vector2.left + Vector2.up) / 2f)) * (Vector2.right + Vector2.down) +
            Vector2.up * view.camera.pixelHeight;

        // Draw the GUI label
        GUI.Label(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);

        // Finish drawing
        UnityEditor.Handles.EndGUI();
        #endif
    }
}