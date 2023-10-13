using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Islands.Generation.IslandGenerator;
using static UnityEngine.EventSystems.EventTrigger;

namespace Islands.Generation
{
    /// <summary>
    /// The central manager for all things island generation
    /// </summary>
    public class IslandGenerator : MonoBehaviour
    {
        public enum SteppingMode
        {
            round,
            floor,
            ceil
        }

        [System.Serializable]
        public struct GeneratorSettings
        {
            [Header("Heightfield Generation")]
            public HeightfieldSettings heightfieldSettings;

            [Header("Heightfield Smoothing")]
            public bool doHeightfieldSmoothing;     // Should smoothing be done at all? Highly recommended depending on perlin scale settings
            public SteppingMode smoothingMode;     // How should the rounding be done? Floor seems to work best for most configurations
            public int smoothIterations;            // How many times should smoothing be done? More than 3 times risks losing a lot of detail

            [Header("Ramps")]
            public bool doRampGeneration;           // Should ramps be generated between the different heights?
            [Range(0f, 1f)] public float rampRatio; // How many of the possible ramp spots should be used?

            [Header("Tile Generation")]
            public bool doTileGeneration;           // Should actual gameObjects be generated for the heightfield? Requires 'Play' mode and a tileset
            public Tileset tileset;

            [Header("Floor Settings")]
            public bool doFloorGeneration;          // Should the floor (aka the bottom of the island) be generated?
            public bool doFloorBorderGeneration;    // Should the floor border (the rocks around the edge of the floor) be generated?
            public FloorSettings floorSettings;

            [Header("Beautification Settings")]
            public bool doBeautification;           // Should flora (+rocks, etc.) be created?
            public Floraset floraset;

            [Header("Navmesh Settings")]
            public bool doNavmeshGeneration;

            [Header("Showcase")]
            public bool setUpShowcase;
        }

        [System.Serializable]
        public struct HeightfieldSettings
        {
            public Vector2Int maxSize;              // Size of the tile grid, square values yield best results
            [Range(0f, 1f)] public float radius;    // Radius of the island compared to the tile grid
            public float radiusOffset;              // Offset that is applied during the radius fall-off calculation
            public float radiusWeight;              // Scalar that defines how heavily radius fall-off is weighted
            public int maxHeight;                   // Maximum island height in tiles
            public Vector2 perlinScale;             // Scale of the perlin noise for the heightfield generation
        }

        [System.Serializable]
        public struct FloorSettings
        {
            public int floorSubdivision;            // How many times should each floor cell be subdivided?
                                                    // 1 for no subdivisions, large values may produce considerable laggs as the floor meshes size increases exponentially

            [Header("Heightfield Settings")]
            public float floorHeightfieldMultiplier;// Scalar for heightfield impact on the floor; currently breaks generation

            [Header("Perlin Settings")]
            public float floorNoiseScale;
            public float floorPerlinMultiplier;     // Scalar for perlin noise impact on the floor 
            public float floorPerlinScale;          // Scale of the floor perlin noise

            [Header("Distance Settings")]
            public float floorDistanceMultiplier;   // Scalar for distance impact on the floor
            public float floorDistanceExponent;     // Exponent for distance impact on the floor

            [Header("Material Setting")]
            public Material floorMaterial;          // The material to use for the floor mesh
        }


        // TODO: Add getters to avoid public variables
        public int seed = -1;

        public GeneratorSettings settings = new GeneratorSettings()
        {
            heightfieldSettings = new HeightfieldSettings()
            {
                maxSize = new Vector2Int(25, 25),
                radius = .75f,
                radiusOffset = 0f,
                radiusWeight = 2f,
                maxHeight = 3,
                perlinScale = new Vector2(3f, 3f)
            },

            doHeightfieldSmoothing = true,
            smoothIterations = 1,

            doRampGeneration = true,
            rampRatio = .1f,

            doTileGeneration = true,

            doFloorGeneration = true,
            doFloorBorderGeneration = true,
            floorSettings = new FloorSettings()
            {
                floorSubdivision = 4,
                floorHeightfieldMultiplier = 0f,

                floorNoiseScale = 1f,
                floorPerlinMultiplier = 2f,
                floorPerlinScale = 10f,

                floorDistanceMultiplier = 10f,
                floorDistanceExponent = 10f
            },

            doBeautification = true,

            doNavmeshGeneration = true,

            setUpShowcase = true
        };


        [Header("Helper variables and references")]
        [SerializeField] private List<Vector3Int> rampSpots;
        [SerializeField] private Texture2D floorHeightfield;
        
        public int[,] heightField;

        [Header("Results - do not edit manually")]
        [SerializeField] private Vector3 genOffset;
        [SerializeField] private Transform islandHolder;
        private List<Transform> tiles = new List<Transform>();

        [Header("Debugging")]
        [SerializeField] private bool printOutput = false;
        [SerializeField] private bool showTileIndices = false;

        /// <summary>
        /// Entry point for island generation
        /// </summary>
        /// <param name="forceRandomSeed"></param>
        public void Generate(bool forceRandomSeed = false)
        {
            StartCoroutine(_Generate(forceRandomSeed));
        }

        /// <summary>
        /// Helper method for island generation. 
        /// Packed as a Coroutine to allow splitting up the process over multiple frames/seconds.
        /// </summary>
        /// <param name="forceRandomSeed"></param>
        /// <returns></returns>
        IEnumerator _Generate(bool forceRandomSeed)
        {
            // Record start time
            var startTime = System.DateTime.Now;

            // Clean up any possible remnants of previous generations
            if (Application.isPlaying)
            {
                if (islandHolder != null)
                    Destroy(islandHolder.gameObject);

                if (tiles == null)
                    tiles = new List<Transform>();
                else
                    tiles.Clear();
            }

            // Set a random seed if required
            if (seed <= 0 || forceRandomSeed)
                seed = (int)(System.DateTime.Now.Ticks % int.MaxValue);

            Utils.Logger.Log("Generating island with seed " + seed);

            // Initialize the generator batch
            var generatorBatch = new Utils.RandomBatch("IslandGeneratorBatch", seed, 100, 0);

            #region Generate Height Field

            // Generate the heightfield
            var heightFieldF = GenerateHeightfield(
                // Center offset in world units
                new Vector2(
                    settings.heightfieldSettings.maxSize.x / 2f, 
                    settings.heightfieldSettings.maxSize.y / 2f),
                // Offset for height field perlin noise
                new Vector2(
                    generatorBatch.NextF() * short.MaxValue,
                    generatorBatch.NextF() * short.MaxValue));

            // Smooth the heightfield
            if (settings.doHeightfieldSmoothing)
            {
                for (int i = 0; i < settings.smoothIterations; i++)
                    SmoothHeightfield(ref heightFieldF);

                if (printOutput)
                    Utils.Logger.LogMatrix(heightFieldF, 1);
            }

            // Convert from gradient heightfield to stepped heightfield
            heightField = ApplyHeightfieldSteps(ref heightFieldF, settings.heightfieldSettings.maxHeight);

            if (printOutput)
                Utils.Logger.LogMatrix(heightField, 1);

            // Mark ramps for later creation
            if (settings.doRampGeneration)
            {
                MarkRamps(ref heightField, generatorBatch);
            }
            #endregion

            #region Generate Tiles

            if (!settings.doTileGeneration)
                yield break;

            if (!Application.isPlaying)
            {
                Utils.Logger.LogError("Cannot generate island while game is not running!");
                yield break;
            }

            if (!settings.tileset)
            {
                Utils.Logger.LogError("Cannot generate island while tileset is not set!");
                yield break;
            }

            GenerateTiles(ref heightField);
            #endregion

            #region Generate Floor & Extras
            yield return null;

            if (settings.doFloorGeneration)
            {
                var floorBorderPoints = GenerateFloor(heightField, heightFieldF, generatorBatch);

                if (settings.doFloorBorderGeneration)
                    GenerateFloorBorder(floorBorderPoints, generatorBatch);
            }

            if (settings.doBeautification)
            {
                GenerateFlora(generatorBatch);
            }

            if (settings.doNavmeshGeneration)
            {
                //FindObjectOfType<AI.NavigationManager>().Bake();
                GenerateNavmesh();
            }
            #endregion

            #region Set up Showcase
            // Set up the showcase manager
            if (settings.setUpShowcase)
            {
                FindObjectOfType<Showcase.ShowcaseManager>().Setup(
                    Vector3.zero /*new Vector3(settings.maxSize.x / 2f * tileset.tileScale.x, settings.maxHeight, settings.maxSize.y / 2f * tileset.tileScale.z)*/, 
                    settings.heightfieldSettings.maxSize.x + settings.heightfieldSettings.maxSize.y);
            }
            #endregion

            // Get the final time taken and notify the editor that we're done
            var endTime = System.DateTime.Now;
            var totalTime = endTime - startTime;

            Utils.Logger.Log($"Finished generation in {totalTime.TotalMilliseconds} ms!");
        }

        /// <summary>
        /// Generates the actual heightfield
        /// </summary>
        /// <param name="center"></param>
        /// <param name="perlinOffset"></param>
        /// <returns></returns>
        float[,] GenerateHeightfield(Vector2 center, Vector2 perlinOffset)
        {
            if (printOutput)
                Utils.Logger.Log("Perlin Offset: " + perlinOffset + "\nHeight Field Center: " + center);

            var heightfieldSettings = settings.heightfieldSettings;

            var heightFieldF = new float[heightfieldSettings.maxSize.x, heightfieldSettings.maxSize.y];

            var maxDistance = heightfieldSettings.radius * center.magnitude;

            for (int i = 0; i < heightFieldF.GetLength(0); i++)
            {
                for (int j = 0; j < heightFieldF.GetLength(1); j++)
                {
                    var iR = (float)i / heightFieldF.GetLength(0);
                    var jR = (float)j / heightFieldF.GetLength(1);

                    // Get raw height
                    var height = Mathf.Clamp01(Mathf.PerlinNoise(
                        iR * heightfieldSettings.perlinScale.x + perlinOffset.x,
                        jR * heightfieldSettings.perlinScale.y + perlinOffset.y));

                    // Apply distance to center fall-off
                    var currentDistance = Vector2.Distance(new Vector2(i, j), center);
                    var distanceMultiplier = (heightfieldSettings.radiusOffset + Mathf.Clamp01(1f - currentDistance / maxDistance)) * heightfieldSettings.radiusWeight;
                    height *= distanceMultiplier;

                    heightFieldF[i, j] = height;
                }
            }



            if (printOutput)
                Utils.Logger.LogMatrix(heightFieldF, 1);

            return heightFieldF;
        }

        /// <summary>
        /// Smooths the heightfield by averaging the 3x3 area around each cell
        /// </summary>
        /// <param name="heightField"></param>
        void SmoothHeightfield(ref float[,] heightField)
        {
            for (int i = 0; i < heightField.GetLength(0); i++)
            {
                for (int j = 0; j < heightField.GetLength(1); j++)
                {
                    float height = heightField[i, j];
                    int c = 1;  // Keep track of the number of utilized cells (usable area may decrease depending on position)

                    if (i > 0)
                    {
                        height += heightField[i - 1, j];
                        c++;
                    }
                    if (j > 0)
                    {
                        height += heightField[i, j - 1];
                        c++;
                    }
                    if (i < heightField.GetLength(0) - 1)
                    {
                        height += heightField[i + 1, j];
                        c++;
                    }
                    if (j < heightField.GetLength(1) - 1)
                    {
                        height += heightField[i, j + 1];
                        c++;
                    }

                    // Debug output to make sure 
                    if (float.IsNaN(height))
                        Debug.LogError($"{heightField[i, j]} [{i},{j}]: {heightField[i - 1, j]},{heightField[i, j - 1]},{heightField[i + 1, j]},{heightField[i, j + 1]}");

                    height = height / c;
                    heightField[i, j] = height;
                }
            }
        }

        /// <summary>
        /// Converts the gradient heightfield with values [0..1] to a step-based heightfield in range [0..steps]
        /// </summary>
        /// <param name="fField">Gradient heightfield</param>
        /// <param name="steps">Number of steps</param>
        /// <returns>Step-based heightfield</returns>
        int[,] ApplyHeightfieldSteps(ref float[,] fField, int steps)
        {
            var iField = new int[fField.GetLength(0), fField.GetLength(1)];

            for (int i = 0; i < fField.GetLength(0); i++)
            {
                for (int j = 0; j < fField.GetLength(1); j++)
                {
                    // Round height and store it
                    var h = Mathf.Lerp(0, steps, fField[i, j]);

                    switch (settings.smoothingMode)
                    {
                        case SteppingMode.round:
                            iField[i, j] = Mathf.RoundToInt(h);
                            break;
                        case SteppingMode.floor:
                            iField[i, j] = Mathf.FloorToInt(h);
                            break;
                        case SteppingMode.ceil:
                            iField[i, j] = Mathf.CeilToInt(h);
                            break;
                    }
                }
            }

            return iField;
        }

        /// <summary>
        /// Mark ramps for later usage
        /// </summary>
        /// <param name="heightField"></param>
        /// <param name="generatorBatch"></param>
        void MarkRamps(ref int[,] heightField, Utils.RandomBatch generatorBatch)
        {
            List<Vector3Int> validRampSpots = new List<Vector3Int>();

            for (int i = 1; i < heightField.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < heightField.GetLength(1) - 1; j++)
                {
                    var c = heightField[i, j];

                    // Check for all spots with two direct equal height neighbours as well as one direct lower height neighbour that are not on cliffs.
                    if (c != 0)
                    {
                        // Check if left/right neighbours are equal
                        if (heightField[i - 1, j] == c && heightField[i + 1, j] == c)
                        {
                            // Check if either up or down spot is lower (yet above height 0) and the other equal
                            if (heightField[i, j + 1] == c && heightField[i, j - 1] < c && heightField[i, j - 1] > 0)
                            {
                                validRampSpots.Add(new Vector3Int(i, j, 0));
                            }
                            else if (heightField[i, j - 1] == c && heightField[i, j + 1] < c && heightField[i, j + 1] > 0)
                            {
                                validRampSpots.Add(new Vector3Int(i, j, 2));
                            }
                        }
                        // Check if up/down neighbours are equal
                        if (heightField[i, j - 1] == c && heightField[i, j + 1] == c)
                        {
                            // Check if either left or right spot is lower (yet above height 0) and the other equal
                            if (heightField[i - 1, j] == c && heightField[i + 1, j] < c && heightField[i + 1, j] > 0)
                            {
                                validRampSpots.Add(new Vector3Int(i, j, 1));
                            }
                            else if (heightField[i + 1, j] == c && heightField[i - 1, j] < c && heightField[i - 1, j] > 0)
                            {
                                validRampSpots.Add(new Vector3Int(i, j, 3));
                            }
                        }
                    }
                }
            }

            // Finally sort out all possible ramp spots
            rampSpots = new List<Vector3Int>();
            validRampSpots.ForEach(spot =>
            {
                // Make sure we keep at least one ramp per rotation and throw away the others randomly
                // Known issue: If there are two unconnected hills on an island, one may be inaccessible if the ramp ratio is set too low
                if (rampSpots.Where(_spot => _spot.z == spot.z).Count() == 0 || generatorBatch.NextF() < settings.rampRatio)
                    rampSpots.Add(spot);
            });

            Debug.Log($"Marked {rampSpots.Count} ramp spots!");
        }

        /// <summary>
        /// Generates the actual tiles from the heightfield
        /// </summary>
        /// <param name="heightField"></param>
        void GenerateTiles(ref int[,] heightField)
        {
            // Set up island and tile holders
            islandHolder = new GameObject("IslandHolder").transform;
            var tileHolder = new GameObject("TileHolder").transform;
            tileHolder.parent = islandHolder;

            for (int i = 0; i < heightField.GetLength(0); i++)
            {
                for (int j = 0; j < heightField.GetLength(1); j++)
                {
                    if (heightField[i, j] > 0f)
                    {
                        // Retrieve the tile instance
                        var newTile = CreateTile(ref heightField, i, j);
                        newTile.parent = tileHolder;

                        tiles.Add(newTile);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method for tile generation which creates a specific tile instance
        /// </summary>
        /// <param name="heightField"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Transform CreateTile(ref int[,] heightField, int x, int y)
        {
            (Tileset.Tile, int) tileInfo;

            // Check if we already declared this position a ramp spot
            if (rampSpots.Find(spot => spot.x == x && spot.y == y) is var rampSpot && rampSpot != Vector3Int.zero)
            {
                tileInfo = (Tileset.Tile.ramp_full, rampSpot.z);
            }
            // Otherwise resolve the tile
            else
            {
                tileInfo = Tileset.ResolveTile(heightField, new Vector2Int(x, y));//Tileset.FindTile(ref heightField, new Vector2Int(x, y));
            }

            // Extract the tile info
            var _tile = tileInfo.Item1;
            var _rot = tileInfo.Item2;

            // Make sure our tiles are centered around 0,0,0
            genOffset.x = -(heightField.GetLength(0) - 1f) * .5f;
            genOffset.y = 0f;
            genOffset.z = -(heightField.GetLength(1) - 1f) * .5f;

            // Instance the tile and init it
            var newTile = settings.tileset.GetTile(_tile, new Vector3Int(x, heightField[x, y], y), _rot);
            newTile.transform.position = Vector3.Scale(settings.tileset.tileScale, new Vector3(x, (heightField[x, y] - 1), y) + genOffset);
            newTile.name = $"Tile_{x}_{y}_{_tile}_{_rot}";
            newTile.AddComponent<Building.TileAnchor>().Init(_tile);

            return newTile;
        }

        /// <summary>
        /// Generate the stone floor below the island
        /// </summary>
        /// <param name="heightField">Step-based height field</param>
        /// <param name="heightFieldF">Gradient height field</param>
        /// <returns></returns>
        List<Vector3> GenerateFloor(int[,] heightField, float[,] heightFieldF, Utils.RandomBatch generatorBatch)
        {
            int w = heightField.GetLength(0);
            int h = heightField.GetLength(1);
            float edge_height = 0f;

            Vector2 uv_max = new Vector2(
                (w) * settings.tileset.tileScale.x,
                (h) * settings.tileset.tileScale.z);

            // Build the floor mesh from scratch
            Mesh floorMesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();

            // Get a direct reference to the floor settings and specific values for sake of brevity
            FloorSettings floorSettings = settings.floorSettings;
            int floorSubdivision = floorSettings.floorSubdivision;

            // Predefine helper values
            Vector2 floorPerlinOffset = new Vector2(generatorBatch.NextF(), generatorBatch.NextF()) * 1000f;
            Vector2 floorCenter = new Vector2(
                w / 2f * floorSubdivision + floorSubdivision / 2f,
                h / 2f * floorSubdivision + floorSubdivision / 2f);
            //uv_max /= 2f;

            var uv_helper = Vector3.Scale(new Vector3(w, 0f, h) + genOffset, settings.tileset.tileScale) - settings.tileset.tileScale / 2f;

            uv_max.x = uv_helper.x;
            uv_max.y = uv_helper.y;

            Vector2Int floorHeightfieldRes = new Vector2Int(w * floorSubdivision, h * floorSubdivision);

            // Define collections to store results later
            Color[] colors = new Color[floorHeightfieldRes.x * floorHeightfieldRes.y];
            List<Vector3> floorBorder = new List<Vector3>();


            #region Local helper methods

            // Sets up the quad for a subdivision cell
            void AddQuad(Vector2Int outerPosition, Vector2Int innerPosition, Vector3 anchor, Vector3 size)
            {
                Vector2 position = new Vector2(
                    outerPosition.x * floorSubdivision + innerPosition.x,
                    outerPosition.y * floorSubdivision + innerPosition.y);

                // Calculate the actual vertex height
                float CalculateFloorHeight(Vector2 uv)
                {
                    var _x = uv.x * floorSettings.floorNoiseScale;
                    var _z = uv.y * floorSettings.floorNoiseScale;

                    var heightFieldModifier = heightFieldF[outerPosition.x, outerPosition.y] * floorSettings.floorHeightfieldMultiplier;
                    var noiseModifier = Mathf.PerlinNoise((floorPerlinOffset.x + uv.x) * floorSettings.floorPerlinScale, (floorPerlinOffset.y + uv.y) * floorSettings.floorPerlinScale) * floorSettings.floorPerlinMultiplier;
                    var distanceModifier = Mathf.Pow(1f - Vector2.Distance(uv, Vector2.one / 2f), floorSettings.floorDistanceExponent) * floorSettings.floorDistanceMultiplier;//floorCenter);

                    if (heightFieldModifier == 0f) heightFieldModifier = 1f;
                    if (noiseModifier == 0f) noiseModifier = 1f;
                    if (distanceModifier == 0f) distanceModifier = 1f;

                    return - heightFieldModifier * noiseModifier * distanceModifier;
                }

                indices.Add(vertices.Count);
                indices.Add(vertices.Count + 1);
                indices.Add(vertices.Count + 2);
                indices.Add(vertices.Count + 3);

                // Calculate vertices
                var p1 = anchor;
                var p2 = anchor + new Vector3(size.x, 0f, 0f);
                var p3 = anchor + new Vector3(size.x, 0f, size.z);
                var p4 = anchor + new Vector3(0f, 0f, size.z);

                // Calculate face uvs from vertices
                var _w = w * floorSubdivision;
                var _h = h * floorSubdivision;

                var uv1 = new Vector2(position.x / _w, position.y / _h);
                var uv2 = new Vector2((position.x + 1) / _w, position.y / _h);
                var uv3 = new Vector2((position.x + 1) / _w, (position.y + 1) / _h);
                var uv4 = new Vector2(position.x / _w, (position.y + 1) / _h);
                //var uv1 = new Vector2(p1.z / uv_max.y, p1.x / uv_max.x);
                //var uv2 = new Vector2(p2.z / uv_max.y, p2.x / uv_max.x);
                //var uv3 = new Vector2(p3.z / uv_max.y, p3.x / uv_max.x);
                //var uv4 = new Vector2(p4.z / uv_max.y, p4.x / uv_max.x);

                // Update vertex heights depending on uvs
                p1.y += CalculateFloorHeight(uv1);
                p2.y += CalculateFloorHeight(uv2);
                p3.y += CalculateFloorHeight(uv3);
                p4.y += CalculateFloorHeight(uv4);

                // Store all vertices and uvs
                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                vertices.Add(p4);

                uvs.Add(uv1);
                uvs.Add(uv2);
                uvs.Add(uv3);
                uvs.Add(uv4);
            }

            // Check if the subdivision is on the left edge
            bool IsOnLeftEdge(int _i, int _j, int __x)
            {
                return (_i == 0 || (heightField[_i - 1, _j] == 0 && __x == 0));
            }

            // Check if the subdivision is on the right edge
            bool IsOnRightEdge(int _i, int _j, int __x)
            {
                return (_i == w - 1 || (heightField[_i + 1, _j] == 0 && __x == floorSubdivision - 1));
            }

            // Check if the subdivision is on the top edge
            bool IsOnTopEdge(int _i, int _j, int __z)
            {
                return (_j == 0 || (heightField[_i, _j - 1] == 0 && __z == 0));
            }
            
            // Check if the subdivision is on the bottom edge
            bool IsOnBotEdge(int _i, int _j, int __z)
            {
                return (_j == h - 1 || (heightField[_i, _j + 1] == 0 && __z == floorSubdivision - 1));
            }

            // Check if the subdivision is on any of the four corners
            int IsOnCorner(int _i, int _j, int __x, int __z)
            {
                //if ((_i == 0 || heightField[_i - 1, _j] == 0) && (_j == 0 || heightField[_i, _j - 1] == 0) && __x == 0 && __z == 0)
                //    return 0;
                if(_i > 0 && _j > 0 && heightField[_i - 1, _j - 1] == 0 && __x == 0 && __z == 0)
                    return 0;
                if(_i < w - 1 && _j > 0 && heightField[_i + 1, _j - 1] == 0 && __x == floorSubdivision - 1 && __z == 0)
                    return 1;
                if (_i < w - 1 && _j < h - 1 && heightField[_i + 1, _j + 1] == 0 && __x == floorSubdivision - 1 && __z == floorSubdivision - 1)
                    return 2;
                if (_i > 0 && _j < h - 1 && heightField[_i - 1, _j + 1] == 0 && __x == 0 && __z == floorSubdivision - 1)
                    return 3;

                return -1;
            }

            #endregion

            // Iterate over the entire heightfield
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (heightField[i, j] == 0)
                        continue;

                    float average_height = heightField[i, j] * settings.tileset.tileScale.y;

                    var centerPos = new Vector3(i, /*-average_height*/ 0f, j);

                    var anchorPos = Vector3.Scale(centerPos + genOffset, settings.tileset.tileScale) - settings.tileset.tileScale / 2f;
                    anchorPos.y = 0f;

                    // Subdivide each heightfield cell and iterate over the specific subdivisions
                    for (int _x = 0; _x < floorSubdivision; _x++)
                    {
                        for (int _z = 0; _z < floorSubdivision; _z++)
                        {
                            float m = heightFieldF[i, j];

                            // Add a quad for the subdivision
                            AddQuad(
                                new Vector2Int(i, j),
                                new Vector2Int(_x, _z),
                                anchorPos + new Vector3(
                                    settings.tileset.tileScale.x / floorSubdivision * _x, 
                                    0f/*-average_height*/, 
                                    settings.tileset.tileScale.z / floorSubdivision * _z), 
                                settings.tileset.tileScale / floorSubdivision);

                            // Force any edge vertices onto the edge height to avoid holes in the islands sides
                            if (_x == 0 && IsOnLeftEdge(i, j, _x))
                            {
                                // For the left edge nullify top left and bottom left vertices
                                var tl = vertices[vertices.Count - 4];
                                var bl = vertices[vertices.Count - 1];

                                m = tl.y = bl.y = edge_height;

                                vertices[vertices.Count - 4] = tl;
                                vertices[vertices.Count - 1] = bl;

                                // Also mark border vertices for additional later processing
                                if (!floorBorder.Contains(tl))
                                    floorBorder.Add(tl);
                                if (!floorBorder.Contains(bl))
                                    floorBorder.Add(bl);
                            }
                            if (_z == 0 && IsOnTopEdge(i, j, _z))
                            {
                                // For the top edge nullify top left and top right vertices
                                var tl = vertices[vertices.Count - 4];
                                var tr = vertices[vertices.Count - 3];

                                m = tl.y = tr.y = edge_height;

                                vertices[vertices.Count - 4] = tl;
                                vertices[vertices.Count - 3] = tr;

                                // Also mark border vertices for additional later processing
                                if (!floorBorder.Contains(tl))
                                    floorBorder.Add(tl);
                                if (!floorBorder.Contains(tr))
                                    floorBorder.Add(tr);
                            }
                            if (_x == floorSubdivision - 1 && IsOnRightEdge(i, j, _x))
                            {
                                // For the right edge nullify top right and bottom right vertices
                                var tr = vertices[vertices.Count - 3];
                                var br = vertices[vertices.Count - 2];

                                m = tr.y = br.y = edge_height;

                                vertices[vertices.Count - 3] = tr;
                                vertices[vertices.Count - 2] = br;

                                // Also mark border vertices for additional later processing
                                if (!floorBorder.Contains(tr))
                                    floorBorder.Add(tr);
                                if (!floorBorder.Contains(br))
                                    floorBorder.Add(br);
                            }
                            if (_z == floorSubdivision - 1 && IsOnBotEdge(i, j, _z))
                            {
                                // For the bottom edge nullify bottom right and bottom left vertices
                                var br = vertices[vertices.Count - 2];
                                var bl = vertices[vertices.Count - 1];

                                m = br.y = bl.y = edge_height;

                                vertices[vertices.Count - 2] = br;
                                vertices[vertices.Count - 1] = bl;

                                // Also mark border vertices for additional later processing
                                if (!floorBorder.Contains(br))
                                    floorBorder.Add(br);
                                if (!floorBorder.Contains(bl))
                                    floorBorder.Add(bl);
                            }
                            if(IsOnCorner(i, j, _x, _z) is var corner && corner >= 0)
                            {
                                // For the corner nullify the specific corner's vertex
                                int index = vertices.Count - (4 - corner);

                                Vector3 v = vertices[index];

                                m = v.y = edge_height;

                                vertices[index] = v;

                                // Also mark border vertices for additional later processing
                                if (!floorBorder.Contains(v))
                                    floorBorder.Add(v);
                            }

                            // Convert the subdivision cell 2d index to 1d and store the colors for the heightfield texture
                            var c_index = i * floorSubdivision + _x + j * w * floorSubdivision * floorSubdivision + _z * w * floorSubdivision;
                            colors[c_index] = Color.white * heightFieldF[i, j];
                            //Debug.Log($"[{i}, {j}, {_x}, {_z}]: Setting c {c_index} to {m}");
                        }
                    }
                }
            }

            // Create heightfield texture for debugging/visualization purposes
            floorHeightfield = new Texture2D(floorHeightfieldRes.x, floorHeightfieldRes.y);
            floorHeightfield.filterMode = FilterMode.Point;

            floorHeightfield.SetPixels(colors);
            floorHeightfield.Apply();

            // Apply heightfield as main texture
            //floorMaterial.mainTexture = floorHeightfield;

            // Set mesh index to UInt32 to allow larger floor meshes
            floorMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            // Apply generated values to mesh
            floorMesh.SetVertices(vertices);
            floorMesh.SetUVs(0, uvs.ToArray());
            floorMesh.SetIndices(indices, MeshTopology.Quads, 0);

            // Recalculate mesh normals, etc.
            floorMesh.RecalculateNormals();
            floorMesh.RecalculateTangents();
            floorMesh.RecalculateBounds();

            // Create object for floor mesh
            var floor = new GameObject("Floor");
            floor.transform.parent = islandHolder;
            floor.transform.localPosition = Vector3.down * settings.tileset.tileScale.y;
            floor.transform.SetAsFirstSibling();
            floor.AddComponent<MeshFilter>().mesh = floorMesh;
            floor.AddComponent<MeshRenderer>().material = floorSettings.floorMaterial;

            return floorBorder;
        }

        /// <summary>
        /// Generate a floor border
        /// </summary>
        /// <param name="borderPositions"></param>
        /// <param name="randomBatch"></param>
        void GenerateFloorBorder(List<Vector3> borderPositions, Utils.RandomBatch randomBatch)
        {
            var positionCount = borderPositions.Count;

            Transform rockHolder = new GameObject("Rock Holder").transform;
            rockHolder.parent = islandHolder;
            rockHolder.SetAsFirstSibling();

            while (borderPositions.Count > 0)
            {
                var position = borderPositions[0];

                var rockEntry = settings.floraset.GetRandomRock(randomBatch);
                var rockInstance = Instantiate(rockEntry.prefab);
                rockInstance.parent = rockHolder;
                rockInstance.rotation = Quaternion.Euler(randomBatch.NextF() * 360f, randomBatch.NextF() * 360f, randomBatch.NextF() * 360f);
                rockInstance.position = position + Vector3.down * settings.tileset.tileScale.y;
                rockInstance.localScale = Vector3.one * (randomBatch.NextF() + .5f);

                borderPositions.RemoveAll(pos => Vector3.Distance(position, pos) < rockEntry.radius);
            }

            Debug.Log($"Placed {rockHolder.childCount} border rocks from {positionCount} different spots");
        }

        /// <summary>
        /// Generate the flora
        /// </summary>
        /// <param name="generatorBatch"></param>
        void GenerateFlora(Utils.RandomBatch generatorBatch)
        {
            // Helper method to retrieve possible flora positions from a tiles surface
            List<Vector3> GatherPositionsFromMesh()
            {
                List<Vector3> positions = new List<Vector3>();

                foreach (var tile in tiles)
                {
                    var tileBoxes = new List<Transform>();
                    float totalVolume = 0f;

                    // Find all tile surfaces from the dedicated surface colliders
                    for (int i = 0; i < tile.childCount; i++)
                    {
                        if (tile.GetChild(i).tag == "TileSurface")
                        {
                            tile.GetChild(i).name = $"{tile.name}_Box_{i}";

                            var box = tile.GetChild(i).GetComponent<BoxCollider>().bounds;

                            totalVolume += box.size.x * box.size.z;

                            tileBoxes.Add(tile.GetChild(i));
                        }
                    }

                    if (tileBoxes.Count == 0)
                    {
                        // This warning indicates tiles that need to be fixed
                        Debug.LogWarning($"Tile {tile.name} has no spawn box! Skipping it...");
                        continue;
                    }

                    // Randomly generate positions from the collected boxes
                    foreach (var box in tileBoxes)
                    {
                        var spawnBoxBounds = box.GetComponent<BoxCollider>().bounds;
                        var volume = spawnBoxBounds.size.x * spawnBoxBounds.size.z;     // Get the volume
                        var max = spawnBoxBounds.max;
                        var min = spawnBoxBounds.min;
                        var delta = max - min;

                        var r = (int)(volume / totalVolume * Floraset.PointsPerTile);   // Get the amount of possible points from this box
                                                                                        // depending on the ratio of the volume compared
                                                                                        // to the other volume boxes of this tile
                        var c = spawnBoxBounds.center;
                        var e = spawnBoxBounds.extents;
                        var s = spawnBoxBounds.size;

                        positions.Add(c);

                        for (int i = 0; i < r; i++)
                        {
                            positions.Add(c - e + new Vector3(
                                generatorBatch.NextF() * s.x,
                                generatorBatch.NextF() * s.y,
                                generatorBatch.NextF() * s.z));
                            /*positions.Add(box.position + box.rotation * spawnBoxBounds.min);/* + new Vector3(
                                Random.value * delta.x,
                                Random.value * delta.y,
                                Random.value * delta.z));
                                */
                        }

                        //Destroy(box.gameObject);
                    }
                }


                return positions;
            }

            // Converts list of possible positions into a list of tuples in the shape of (flora prefab, position to place in, radius of the object)
            List<(Transform, Vector3, float)> GenerateFloraPositions(List<Vector3> meshPositions)
            {
                var positions = new List<(Transform, Vector3, float)>();

                while (meshPositions.Count > 0)
                {
                    var index = Random.Range(0, meshPositions.Count);

                    var floraEntry = settings.floraset.GetRandomFlora(generatorBatch);

                    positions.Add((floraEntry.prefab, meshPositions[index], floraEntry.radius));

                    // Remove all nearby flora positions to avoid overlapping mehses
                    //meshPositions.RemoveAt(index);
                    meshPositions.RemoveAll(position => Vector3.Distance(position, meshPositions[index]) <= floraEntry.radius);
                }

                return positions;
            }

            // Actually instance all flora objects
            void GenerateFloraInstances(List<(Transform, Vector3, float)> floraPositions)
            {
                Transform floraHolder = new GameObject("Flora Holder").transform;
                floraHolder.parent = islandHolder;
                floraHolder.SetAsFirstSibling();

                foreach (var entry in floraPositions)
                {
                    var newInst = Instantiate(entry.Item1);
                    newInst.parent = floraHolder;
                    newInst.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);
                    newInst.position = entry.Item2;
                }
            }

            // First get all potential flora positions
            List<Vector3> validPositions = GatherPositionsFromMesh();
            Debug.Log($"Found {validPositions.Count} total viable positions across {tiles.Count} tiles  !");

            // Then generate actual entries while paying attention to potential overlapping
            List<(Transform, Vector3, float)> positionEntries = GenerateFloraPositions(validPositions);
            Debug.Log($"Found {positionEntries.Count} final flora objects to place!");

            // Finally instance the flora objects
            GenerateFloraInstances(positionEntries);
            Debug.Log($"Placed a total of {islandHolder.Find("Flora Holder").childCount} flora objects!");
        }

        /// <summary>
        /// Generate the navmesh after a short delay.
        /// Navmesh settings are managed via the NavMeshSurface on this GameObject
        /// </summary>
        void GenerateNavmesh()
        {
            //GetComponent<NavMeshSurface>().BuildNavMesh();
            StartCoroutine(_DelayedNavmeshBake());
        }

        // Navmesh helper
        IEnumerator _DelayedNavmeshBake()
        {
            yield return new WaitForSeconds(1f);
            GetComponent<NavMeshSurface>().BuildNavMesh();
            Debug.Log("Baked navmesh!");
        }

        private void OnDrawGizmos()
        {
            if (heightField == null || !showTileIndices)
                return;

            // Draw tile indices
            for (int i = 0; i < heightField.GetLength(0); i++)
            {
                for (int j = 0; j < heightField.GetLength(1); j++)
                {
                    if (heightField[i, j] > 0f)
                    {
                        DebugUtils.DrawString(
                            $"{i},{j}",
                            new Vector3(
                                i * settings.tileset.tileScale.x, 
                                (heightField[i, j] - 1) * settings.tileset.tileScale.y, 
                                j * settings.tileset.tileScale.z),
                            Color.red,
                            Vector2.zero);
                    }
                }
            }
        }
    }
}