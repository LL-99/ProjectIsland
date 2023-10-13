using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for the Island Generator that adds some easy testing functionalities via the inspector
/// </summary>
[CustomEditor(typeof(Islands.Generation.IslandGenerator))]
public class IslandGeneratorEditor : Editor
{
    static bool forceRandomSeed = false;    // Should we force a new seed every time we generate a new island?
    static float autoUpdate = 0f;           // If greater than 0, trigger island generation automatically every so often
    DateTime lastAutoUpdate;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Autogen in this gui step?
        bool autogen = autoUpdate > 0f && (DateTime.Now - lastAutoUpdate).TotalSeconds > autoUpdate;

        // Trigger island generation
        if(GUILayout.Button("Generate Island") || autogen)
        {
            // Reset update timer
            lastAutoUpdate = DateTime.Now;

            // Get generator
            Islands.Generation.IslandGenerator generator = target as Islands.Generation.IslandGenerator;

            // If we're automatically generating disable navmesh generation as its 1 sec delay can cause issues/artifacts otherwise
            if (autogen)
                generator.settings.doNavmeshGeneration = false;

            // Generate
            generator.Generate(forceRandomSeed);

            // Extract heightfield data and back it up
            var hf = generator.heightField;
            var tex = new Texture2D(hf.GetLength(0), hf.GetLength(1));
            var colors = new List<Color>();

            for (int i = 0; i < tex.width; i++)
            {
                for (int j = 0; j < tex.height; j++)
                {
                    var v = (hf[i, j] / (float)generator.settings.heightfieldSettings.maxHeight);
                    colors.Add(new Color(v, v, v, 1f));
                    //tex.SetPixel(i, j, new Color(v, v, v, 1f));
                }
            }

            tex.SetPixels(colors.ToArray());
            tex.Apply();

            var bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/Textures/Generation/LatestHeightfield.png", bytes);
            AssetDatabase.Refresh();
        }

        // Handle the additional settings
        forceRandomSeed = GUILayout.Toggle(forceRandomSeed, "Force random seed?");
        autoUpdate = EditorGUILayout.FloatField("Auto Update Rate (delay in seconds)", autoUpdate);
    }
}
