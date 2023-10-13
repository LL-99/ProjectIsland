using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Islands.Generation
{
    /// <summary>
    /// Contains references to every single possible marching square tile that may be used during island creation
    /// </summary>
    [CreateAssetMenu(fileName = "New Tileset", menuName = "Custom/New Tileset")]
    public class Tileset : ScriptableObject
    {
        // Based on the good old reliable marching squares
        public enum Tile
        {
            none,
            full,
            pillar,                 // four edges / outer corners -> all empty/lower around

            corner_inner_single,
            corner_inner_double_adjacent,
            corner_inner_double_opposite,
            corner_inner_triple,
            corner_inner_quadruple,

            edge_single,
            edge_double_opposite,   // includes bridges

            corner_inner_single_edge_single,
            corner_inner_single_edge_single_mirrored,

            corner_outer_single,    // also double edge adjacent
            corner_outer_double,    // also triple edge
            corner_inner_corner_outer,

            ramp_full
        }

        //struct Pattern
        //{
        //    int[,] values;

        //    public Pattern(int[,] values)
        //    {
        //        this.values = values;
        //    }

        //    public bool Evaluate(ref int[,] heightField, int rotation)
        //    {
        //        int[,] v = new int[3,3];

        //        for (int i = 0; i < 3; i++)
        //        {
        //            for (int j = 0; j < 3; j++)
        //            {
        //                switch(rotation)
        //                {
        //                    case 0:
        //                        v[i, j] = heightField[i, j];
        //                        break;
        //                    case 1:
        //                        v[i, j] = heightField[j, 2 - i];
        //                        break;
        //                }
        //            }
        //        }

        //        for (int i = 0; i < 3; i++)
        //        {
        //            for(int j = 0; j < 3; j++)
        //            {

        //            }
        //        }
        //    }
        //}

        static List<(Tile, int[])> tileLayouts = new List<(Tile, int[])>()
        {
            (Tile.none,
                new int[]
                {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                }),
            (Tile.full,
                new int[]
                {
                    2, 2, 2,
                    2, 1, 2,
                    2, 2, 2,
                }),
            (Tile.pillar,
                new int[]
                {
                    0, 0, 0,
                    0, 1, 0,
                    0, 0, 0,
                }),

            (Tile.corner_inner_single,
                new int[]
                {
                    0, 1, 2,
                    1, 1, 2,
                    2, 2, 2,
                }),
            (Tile.corner_inner_double_adjacent,
                new int[]
                {
                    0, 1, 0,
                    1, 1, 1,
                    2, 2, 2,
                }),
            (Tile.corner_inner_double_opposite,
                new int[]
                {
                    0, 1, 2,
                    1, 1, 1,
                    2, 1, 0,
                }),
            (Tile.corner_inner_triple,
                new int[]
                {
                    0, 1, 0,
                    1, 1, 1,
                    2, 1, 0,
                }),
            (Tile.corner_inner_quadruple,
                new int[]
                {
                    0, 1, 0,
                    1, 1, 1,
                    0, 1, 0,
                }),

            (Tile.edge_single,
                new int[]
                {
                    -1, 0, -1,
                    2, 1, 2,
                    2, 2, 2,
                }),
            (Tile.edge_double_opposite,
                new int[]
                {
                    2, 0, 2,
                    2, 1, 2,
                    2, 0, 2,
                }),

            (Tile.corner_inner_single_edge_single,
                new int[]
                {
                    0, 2, -1,
                    1, 1, 0,
                    2, 2, -1,
                }),
            (Tile.corner_inner_single_edge_single_mirrored,
                new int[]
                {
                    2, 2, -1,
                    1, 1, 0,
                    0, 2, -1,
                }),

            (Tile.corner_outer_single,
                new int[]
                {
                    -1, 0, -1,
                    2, 1, 0,
                    2, 2, -1,
                }),
            (Tile.corner_outer_double,
                new int[]
                {
                    -1, 0, -1,
                    2, 1, 0,
                    -1, 0, -1,
                }),
            (Tile.corner_inner_corner_outer,
                new int[]
                {
                    -1, 0, -1,
                    1, 1, 0,
                    0, 1, -1,
                }),
        };

        public Vector3 tileScale = new Vector3(4f, 4f, 4f);

        [Header("References")]

        public Transform tile_none;
        public Transform tile_full;
        public Transform tile_pillar;

        public Transform corner_inner_single;
        public Transform corner_inner_double_adjacent;
        public Transform corner_inner_double_opposite;
        public Transform corner_inner_triple;
        public Transform corner_inner_quadruple;

        public Transform edge_single;
        public Transform edge_double_opposite;

        public Transform corner_inner_single_edge_single;
        public Transform corner_inner_single_edge_single_mirrored;

        public Transform corner_outer_single;
        public Transform corner_outer_double;
        public Transform corner_inner_corner_outer;

        public Transform ramp_full;

        /// <summary>
        /// Returns the respective tile prefab for any given 'Tile' value
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public Transform GetTilePrefab(Tile tile)
        {
            switch (tile)
            {
                case Tile.none:
                    return tile_none;
                case Tile.full:
                    return tile_full;
                case Tile.pillar:
                    return tile_pillar;

                case Tile.corner_inner_single:
                    return corner_inner_single;
                case Tile.corner_inner_double_adjacent:
                    return corner_inner_double_adjacent;
                case Tile.corner_inner_double_opposite:
                    return corner_inner_double_opposite;
                case Tile.corner_inner_triple:
                    return corner_inner_triple;
                case Tile.corner_inner_quadruple:
                    return corner_inner_quadruple;

                case Tile.edge_single:
                    return edge_single;
                case Tile.edge_double_opposite:
                    return edge_double_opposite;

                case Tile.corner_inner_single_edge_single:
                    return corner_inner_single_edge_single;
                case Tile.corner_inner_single_edge_single_mirrored:
                    return corner_inner_single_edge_single_mirrored;

                case Tile.corner_outer_single:
                    return corner_outer_single;
                case Tile.corner_outer_double:
                    return corner_outer_double;
                case Tile.corner_inner_corner_outer:
                    return corner_inner_corner_outer;

                case Tile.ramp_full:
                    return ramp_full;
            }

            return null;
        }

        /// <summary>
        /// Creates a new tile
        /// </summary>
        /// <param name="tile">The 'Tile' to create</param>
        /// <param name="tilePosition">The position of the tile</param>
        /// <param name="tileRotation">The orientation of the tile</param>
        /// <returns>The new instance</returns>
        public Transform GetTile(Tile tile, Vector3Int tilePosition, int tileRotation)
        {
            Transform prefab = GetTilePrefab(tile);

            if(prefab == null)
            {
                Debug.LogWarning($"Tile {tile} not set! Using empty tile instead!");
                prefab = GetTilePrefab(Tile.none);
            }

            var newInst = Instantiate(prefab);
            newInst.position = new Vector3(
                tileScale.x * tilePosition.x,
                tileScale.y * tilePosition.y,
                tileScale.z * tilePosition.z);
            newInst.rotation = Quaternion.Euler(0f, -tileRotation * 90f, 0f);

            return newInst;
        }

        /// <summary>
        /// Converts any given heightfield position into a tile and rotation
        /// </summary>
        /// <param name="heightField"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static (Tile, int) ResolveTile(int[,] heightField, Vector2Int pos)
        {
            // Precompute / -define important variables
            int centerHeight = heightField[pos.x, pos.y];
            int w = heightField.GetLength(0) - 1;
            int h = heightField.GetLength(1) - 1;

            int[] localHeightField = new int[9];

            #region Local functions

            // Converts actual heightfield height to a relative value (0 = below, 1 = equal height, 2 = above) for eased pattern matching
            int GetNormalizedHeight(int _x, int _y)
            {
                int heightAtPos = heightField[_x, _y];

                if (heightAtPos == centerHeight)
                    return 1;
                if (heightAtPos > centerHeight)
                    return 2;

                // heightAtPos < centerHeight
                return 0;
            }

            // Matches the local heightfield against all possible rotations
            int MatchHeightfield(int[] heightField)
            {
                for (int i = 0; i < 4; i++)
                {
                    var result = MatchingHelper(heightField, i);

                    if (result > -1)
                        return i;
                }

                return -1;
            }

            // Rotation matching helper
            int MatchingHelper(int[] heightField, int rotation)
            {
                var rotatedHeightField = GetRotatedHeightField(heightField, rotation);

                for (int i = 0; i < 9; i++)
                {
                    if (rotatedHeightField[i] != localHeightField[i] &&
                        !(rotatedHeightField[i] == 2 && localHeightField[i] >= 1) &&
                        !(rotatedHeightField[i] == -1))
                        return -1;
                }
                return 0;
            }

            // Rotates a 2d heightfield in 90° steps
            int[] GetRotatedHeightField(int[] heightField, int rotation)
            {
                int[] rotatedHeightField = new int[9];

                for (int i = 0; i < 9; i++)
                {
                    switch (rotation)
                    {
                        case 0:
                            rotatedHeightField[i] = heightField[i];
                            break;
                        case 1: // 90° clock-wise
                            rotatedHeightField[i] = heightField[GetRotatedIndex(i, rotation)];
                            break;
                        case 2: // 180° clock-wise (inverted)
                            rotatedHeightField[i] = heightField[8 - i];
                            break;
                        case 3: // 270° clock-wise / 90° counter clock-wise
                            rotatedHeightField[i] = heightField[GetRotatedIndex(i, rotation)];
                            break;
                    }
                }

                return rotatedHeightField;
            }

            // Rotation helper
            int GetRotatedIndex(int index, int rotation)
            {
                Vector3 position = new Vector3(index % 3 - 1, 0, index / 3 - 1);

                var rot = Quaternion.Euler(0f, rotation * 90f, 0f);

                var newPos = rot * position;

                var newIndex = Mathf.RoundToInt(newPos.x + 1 + (newPos.z + 1) * 3);
                //Debug.Log($"Rotating index {index} by {rotation * 90}° to {newIndex}");

                return newIndex;
            }

            #endregion

            // Set up local height field
            localHeightField[0] = pos.x > 0 && pos.y > 0 ? GetNormalizedHeight(pos.x - 1, pos.y - 1) : 0;
            localHeightField[1] = pos.y > 0 ? GetNormalizedHeight(pos.x, pos.y - 1) : 0;
            localHeightField[2] = pos.x < w && pos.y > 0 ? GetNormalizedHeight(pos.x + 1, pos.y - 1) : 0;
            
            localHeightField[3] = pos.x > 0 ? GetNormalizedHeight(pos.x - 1, pos.y) : 0;
            localHeightField[4] = 1;
            localHeightField[5] = pos.x < w ? GetNormalizedHeight(pos.x + 1, pos.y) : 0;

            localHeightField[6] = pos.x > 0 && pos.y < h ? GetNormalizedHeight(pos.x - 1, pos.y + 1) : 0;
            localHeightField[7] = pos.y < h ? GetNormalizedHeight(pos.x, pos.y + 1) : 0;
            localHeightField[8] = pos.x < w && pos.y < h ? GetNormalizedHeight(pos.x + 1, pos.y + 1) : 0;

            // Match local heightfield against tile definitions (equivalent of a marching square table lookup)
            var validLayouts = new List<(Tile, int)>();

            tileLayouts.ForEach(tuple =>
            {
                var result = MatchHeightfield(tuple.Item2);

                if (result > -1)
                    validLayouts.Add((tuple.Item1, result));
            });

            // Return the valid layout
            if(validLayouts.Count > 0)
            {
                // Fringe case used for debugging purposes
                if (validLayouts.Count > 1)
                {
                    Debug.LogWarning($"FOUND MORE THAN 1 LAYOUT! ({validLayouts.Count} for {pos.x}/{pos.y}");
                    Utils.Logger.LogMatrix(localHeightField, 3);
                }

                return validLayouts[0];
            }

            // This should never happen; left here for debugging purposes
            Debug.LogWarning($"Found NO valid layouts for {pos.x}/{pos.y}");
            //Debug.Log("Surrounding normalized height field was: ");
            Utils.Logger.LogMatrix(localHeightField, 3);

            return (Tile.none, 0);
        }

        [Obsolete]
        public static (Tile, int) FindTile(ref int[,] heightField, Vector2Int pos)
        {
            bool tl, t, tr, cl, c, cr, bl, b, br;
            tl = t = tr = cl = c = cr = bl = b = br = false;

            int _c = heightField[pos.x, pos.y];
            int w = heightField.GetLength(0) - 1;
            int h = heightField.GetLength(1) - 1;

            try
            {
                tl = pos.x > 0 && pos.y > 0 && heightField[pos.x - 1, pos.y - 1] >= _c;
                t = pos.y > 0 && heightField[pos.x, pos.y - 1] >= _c;
                tr = pos.x < w && pos.y > 0 && heightField[pos.x + 1, pos.y - 1] >= _c;

                cl = pos.x > 0 && heightField[pos.x - 1, pos.y] >= _c;
                c = heightField[pos.x, pos.y] > 0;
                cr = pos.x < w && heightField[pos.x + 1, pos.y] >= _c;

                bl = pos.x > 0 && pos.y < h && heightField[pos.x - 1, pos.y + 1] >= _c;
                b = pos.y < h && heightField[pos.x, pos.y + 1] >= _c;
                br = pos.x < w && pos.y < h && heightField[pos.x + 1, pos.y + 1] >= _c;
            }
            catch (Exception e)
            {
                throw e;
            }

            return FindTile(tl, t, tr, cl, c, cr, bl, b, br);
        }

        [Obsolete]
        public static (Tile, int) FindTile(bool tl, bool t, bool tr, bool cl, bool c, bool cr, bool bl, bool b, bool br)
        {
            int lessCount =
                (!tl ? 1 : 0) + (!t ? 1 : 0) + (!tr ? 1 : 0) +
                (!cl ? 1 : 0) + (!c ? 1 : 0) + (!cr ? 1 : 0) +
                (!bl ? 1 : 0) + (!b ? 1 : 0) + (!br ? 1 : 0);
            int lessCount_edges =
                (!t ? 1 : 0) + (!cr ? 1 : 0) + (!b ? 1 : 0) + (!cl ? 1 : 0);
            int lessCount_corners =
                (!tl ? 1 : 0) + (!tr ? 1 : 0) + (!br ? 1 : 0) + (!bl ? 1 : 0);

            if (!c)
                return (Tile.none, 0);

            switch (lessCount)
            {
                case 8:
                    return (Tile.pillar, 0);

                case 7:
                case 6:
                    if (cl && bl)
                        return (Tile.corner_inner_corner_outer, 0);
                    if (cl && t)
                        return (Tile.corner_inner_corner_outer, 1);
                    if (t && cr)
                        return (Tile.corner_inner_corner_outer, 2);
                    if (cr && b)
                        return (Tile.corner_inner_corner_outer, 3);

                    if (cl)
                        return (Tile.corner_outer_double, 0);
                    if (t)
                        return (Tile.corner_outer_double, 1);
                    if (cr)
                        return (Tile.corner_outer_double, 2);
                    if (b)
                        return (Tile.corner_outer_double, 3);
                    break;

                case 5:
                case 4:
                    if (tl && tr && br && bl)
                        return (Tile.corner_inner_quadruple, 0);
                    if (t && cr && b && cl)
                        return (Tile.pillar, 0);

                    if (b && cl)
                        return (Tile.corner_outer_single, 0);
                    if (cl && t)
                        return (Tile.corner_outer_single, 1);
                    if (t && cr)
                        return (Tile.corner_outer_single, 2);
                    if (cr && b)
                        return (Tile.corner_outer_single, 3);
                    break;

                case 3:
                    // 6, 7 && 3
                    if (lessCount_corners >= 3)
                    {
                        if (bl)
                            return (Tile.corner_inner_triple, 0);
                        if (tl)
                            return (Tile.corner_inner_triple, 1);
                        if (tr)
                            return (Tile.corner_inner_triple, 2);
                        if (br)
                            return (Tile.corner_inner_triple, 3);
                    }
                    if (lessCount_edges >= 3)
                    {
                        if (t)
                            return (Tile.corner_outer_double, 0);
                        if (cr)
                            return (Tile.corner_outer_double, 1);
                        if (b)
                            return (Tile.corner_outer_double, 2);
                        if (cl)
                            return (Tile.corner_outer_double, 3);
                    }
                    break;

                case 1:
                    if (tl)
                        return (Tile.corner_inner_single, 0);
                    if (tr)
                        return (Tile.corner_inner_single, 1);
                    if (br)
                        return (Tile.corner_inner_single, 2);
                    if (bl)
                        return (Tile.corner_inner_single, 3);
                    break;
                case 0:
                    return (Tile.full, 0);

            }

            return (Tile.none, 0);
        }


        
    }
}