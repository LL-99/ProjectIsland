using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Islands.Building
{
    /// <summary>
    /// Handles all things related to buildings
    /// Currently doesn't have functionality beyond selecting tiles, but will be expanded in the future
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        #region Singleton
        public static BuildingManager Instance { get; private set; }

        public void EnforceSingleton()
        {
            if (!Instance) Instance = this;
            else if (Instance != this) Destroy(this);
        }
        #endregion

        [SerializeField] private TileCursor tileCursor;
        [SerializeField] private TileAnchor selectedTile;

        private void Awake()
        {
            EnforceSingleton();
        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                HandleSelect();
            }
        }

        /// <summary>
        /// Raycasts from camera position through cursor to find selected tiles
        /// </summary>
        public void HandleSelect()
        {
            var cursorScreenPosition = Input.mousePosition;
            cursorScreenPosition.z = 1f;

            var cursorRay = Camera.main.ScreenPointToRay(cursorScreenPosition);

            var hits = Physics.RaycastAll(cursorRay);

            Debug.Log($"Hit {hits.Length} colliders at {cursorScreenPosition}");

            foreach(var hit in hits )
            {
                Debug.Log($"{hit.collider.name}");

                if(hit.collider.GetComponentInParent<IClickable>() is var clickable && clickable != null)
                {
                    clickable.Click();
                    break;
                }
            }
        }

        /// <summary>
        /// Selects any given tile and returns if it has been selected successfuly
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public bool SelectTile(TileAnchor tile)
        {
            var couldMoveCursor = tileCursor.MoveCursor(tile.transform.position);

            if(!couldMoveCursor)
                return false;

            // TODO: Show UI
            selectedTile = tile;

            return true;
        }
    }
}