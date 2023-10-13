using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Islands.Building
{
    /// <summary>
    /// Data storage placed upon each tile once
    /// Also implements the IClickable interface to make the tile selectable
    /// </summary>
    public class TileAnchor : MonoBehaviour, IClickable
    {
        public enum TileState
        {
            vacant,
            blocked,
            occupied
        }

        [SerializeField] private Generation.Tileset.Tile tileType;
        [SerializeField] private TileState state;
        [SerializeField] private Transform tileMesh;
        [SerializeField] private Transform floraHolder;

        public Generation.Tileset.Tile Tile => tileType;

        public (Mesh, UnityEngine.Rendering.SubMeshDescriptor) GetGrassSurface()
        {
            var _mesh = tileMesh.GetComponent<MeshFilter>().sharedMesh;
            return (_mesh, _mesh.GetSubMesh(0));
        }

        public void Init(Generation.Tileset.Tile tileType)
        {
            this.tileType = tileType;

            tileMesh = transform.GetChild(0);
        }

        public void Click()
        {
            SelectTile();
        }

        private void SelectTile()
        {
            Debug.Log($"Clicked tile {transform.name}");
            BuildingManager.Instance.SelectTile(this);
        }
    }
}