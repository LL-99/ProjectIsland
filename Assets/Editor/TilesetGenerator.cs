using Islands.Generation;
using System;
using UnityEditor;
using UnityEngine;
using static Islands.Generation.Tileset;

[Obsolete("Due a tileset rework this generator has become obsolete and is no longer functional or maintained")]
public class TilesetGenerator : EditorWindow
{
    Tileset currentTileset;
    Tile currentTile;

    //[MenuItem("Tools/Tileset Generator")]
    public static void ShowMyEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<TilesetGenerator>();
        wnd.titleContent = new GUIContent("Tileset Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hi");

        EditorGUILayout.BeginHorizontal();

        Tileset tileset = EditorGUILayout.ObjectField("", currentTileset, typeof(Tileset), true) as Tileset;

        if (GUILayout.Button("New"))
        {
            CreateNewTileset();
        }

        if (tileset != currentTileset || GUILayout.Button("Load"))
        {
            currentTileset = tileset;
            UpdateTilesetData();
        }

        EditorGUILayout.EndHorizontal();

        // Current Tile
        currentTile = (Tile)EditorGUILayout.EnumPopup("Tile: ", currentTile);

        GUILayout.BeginVertical();
        for(int i = 0; i < 3; i++)
        {
            GUILayout.BeginHorizontal();

            for (int j = 0; j < 3; j++)
            {
                DrawTile(i * 3 + j);
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void CreateNewTileset()
    {
        var tilesetName = $"Tileset_{ System.DateTime.Now.Ticks}";
        var path = $"Assets/ScriptableObjects/Tilesets/{tilesetName}.asset";
        currentTileset = CreateInstance<Tileset>();
        currentTileset.name = tilesetName;

        AssetDatabase.CreateAsset(currentTileset, path);
        AssetDatabase.Refresh();

        currentTileset = AssetDatabase.LoadAssetAtPath<Tileset>(path);
    }

    void UpdateTilesetData()
    {

    }

    void DrawTile(int tileIndex)
    {
        GUILayout.Button(tileIndex + "");


    }
}
